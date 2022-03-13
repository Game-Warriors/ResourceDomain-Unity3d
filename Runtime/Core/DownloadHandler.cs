using GameWarriors.ResourceDomain.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GameWarriors.ResourceDomain.Core
{
    public class DownloadHandler
    {
        private enum EDownloadState
        {
            None,
            GetInfo,
            Downloading,
            Stop,
            Complete
        }

        private readonly int _bufferSize;
        private readonly Semaphore _orderManager;
        private readonly Stack<byte[]> _bytePool;
        private readonly Queue<DownloadPack> _dataPackQueue;

        private long _currentPos;
        private long _finalPos;
        private long _downloadBytes;
        private EDownloadState _state;

        public DateTime StartDate { get; private set; }
        public long CurrentPos => _currentPos;
        public long FinalPos => _finalPos;
        public long DownloadBytes => _downloadBytes;

        public bool IsComplete => _state == EDownloadState.Complete;
        public bool IsStop => _state == EDownloadState.Stop || _state == EDownloadState.None;

        public bool IsRunning => _state == EDownloadState.Downloading || _state == EDownloadState.GetInfo;
        public bool IsAutoReconnect { get; set; }
        public bool IsDownloading => _state == EDownloadState.Downloading;

        public DownloadHandler(int bufferSize)
        {
            _bufferSize = bufferSize;
            _orderManager = new Semaphore(0, 5000);
            int poolSize = 5;
            _bytePool = new Stack<byte[]>(poolSize * 2);
            for (int i = 0; i < poolSize; ++i)
            {
                _bytePool.Push(new byte[bufferSize]);
            }

            _dataPackQueue = new Queue<DownloadPack>(poolSize * 2);
        }

        public async Task StarProcess(string url, string primaryFileName, Action<bool> onStart)
        {
            try
            {
                do
                {
                    StartDate = DateTime.UtcNow;
                    _state = EDownloadState.GetInfo;
                    (string, long) result = await GetFileName(url, primaryFileName);
                    string fileName = result.Item1;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        _state = EDownloadState.Stop;
                        onStart?.Invoke(false);
                        return;
                    }

                    using StreamWriter writer = new StreamWriter(File.Open(fileName, FileMode.OpenOrCreate));
                    onStart?.Invoke(true);
                    _state = EDownloadState.Downloading;
                    var fileLength = writer.BaseStream.Length;
                    if (fileLength == _finalPos)
                        _currentPos = fileLength;
                    else
                        _currentPos = (writer.BaseStream.Length + writer.BaseStream.Position) / 2;

                    _finalPos = result.Item2;
                    _downloadBytes = 0;

                    //writer.BaseStream.Seek(0, SeekOrigin.End);
                    Task downloadTask = DownloadProcess(url, fileLength, _finalPos);
                    Task fileWritingTask = Task.Factory.StartNew(() => WriteFileProcess(writer));
                    Task endTask = await Task.WhenAny(downloadTask, fileWritingTask);
                    if (endTask == downloadTask)
                    {
                        _orderManager.Release();
                        await Task.WhenAny(fileWritingTask, Task.Delay(6000));
                    }
                    else
                    {
                        await Task.WhenAny(downloadTask, Task.Delay(6000));
                    }

                    //fileLength = writer.BaseStream.Length;
                    //UnityEngine.Debug.Log("file length : " + fileLength);
                } while (IsStop && IsAutoReconnect);
            }
            catch
            {
                _state = EDownloadState.Stop;
            }
            finally
            {
                if (_dataPackQueue != null)
                {
                    int length = _dataPackQueue.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        var tmp = _dataPackQueue.Dequeue();
                        _bytePool.Push(tmp.Data);
                    }
                }
            }
        }

        public void StopDownload()
        {
            IsAutoReconnect = false;
            _state = EDownloadState.Stop;
        }


        public void Reset()
        {
            _state = EDownloadState.None;
            _currentPos = 0;
            _finalPos = 0;
            _downloadBytes = 0;
        }


        public void ResetByteCount()
        {
            lock (this)
                _downloadBytes = 0;
        }

        private async Task<(string, long)> GetFileName(string url, string fileName)
        {
            try
            {
                HttpWebRequest fileReq = (HttpWebRequest) WebRequest.Create(url);
                fileReq.Method = "Head";
                //fileResp.Headers.Add("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
                //fileResp.Headers.Add("Content-Length", fileResp.ContentLength.ToString());
                using HttpWebResponse fileResp = (HttpWebResponse) await fileReq.GetResponseAsync();
                string headerValue = fileResp.Headers.Get("Content-Disposition");
                long contentLength = fileResp.ContentLength;
                if (!string.IsNullOrEmpty(headerValue))
                {
                    int extensionIndex = headerValue.LastIndexOf('.');
                    string extension = null;
                    if (extensionIndex > 0)
                    {
                        extension = headerValue.Substring(extensionIndex, headerValue.Length - extensionIndex);
                    }

                    if (string.IsNullOrEmpty(fileName))
                    {
                        int fileNameIndex = headerValue.LastIndexOf('=');
                        for (int i = fileNameIndex; i < headerValue.Length - 1; ++i)
                        {
                            char tmp = headerValue[i];
                            char tmp2 = headerValue[i];
                            if (tmp == 39 && tmp2 == 39)
                            {
                                fileNameIndex = i + 2;
                                break;
                            }
                        }

                        if (fileNameIndex > 0)
                        {
                            fileName = headerValue.Substring(fileNameIndex,
                                headerValue.Length - fileNameIndex - extension.Length);
                        }
                        else
                            fileName = Guid.NewGuid().ToString();
                    }

                    return (fileName + extension, contentLength);
                }
                else
                {
                    headerValue = fileResp.ContentType;
                    if (string.IsNullOrEmpty(headerValue))
                    {
                        string name = string.IsNullOrEmpty(fileName) ? Guid.NewGuid().ToString() : fileName;
                        return (name, contentLength);
                    }

                    int extensionIndex = headerValue.LastIndexOf('/');
                    ++extensionIndex;
                    return string.IsNullOrEmpty(fileName)
                        ? (
                            $"{Guid.NewGuid()}.{headerValue.Substring(extensionIndex, headerValue.Length - extensionIndex)}",
                            contentLength)
                        : ($"{fileName}.{headerValue.Substring(extensionIndex, headerValue.Length - extensionIndex)}",
                            contentLength);
                }
            }
            catch (Exception E)
            {
                string ex = E.ToString();
                return (null, 0);
            }
        }

        private async Task DownloadProcess(string fileURL, long startPos, long contentLength)
        {
            //Create a stream for the file
            Stream stream = null;
            try
            {
                //Create a WebRequest to get the file
                HttpWebRequest fileReq = (HttpWebRequest) WebRequest.Create(fileURL);
                fileReq.Method = "Get";
                fileReq.ContentType = "application/octet-stream";
                //Create a response for this request
                using HttpWebResponse fileResp = (HttpWebResponse) await fileReq.GetResponseAsync();
                _finalPos = contentLength;
                if (startPos >= _finalPos)
                {
                    _state = EDownloadState.Complete;
                    return;
                }

                //Get the Stream returned from the response
                stream = fileResp.GetResponseStream();
                if (startPos > 0)
                {
                    fileReq.AddRange(startPos);
                }

                do
                {
                    byte[] buffer = GetBuffer();
                    if (stream == null) continue;
                    int length = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (!IsDownloading)
                        break;

                    if (length > 0)
                    {
                        _downloadBytes += length;
                        PushDownloadData(new DownloadPack(buffer, length));
                        _orderManager.Release();
                    }
                    else
                    {
                        ReleaseBuffer(buffer);
                        break;
                    }
                } while (true);
            }
            catch //(Exception E)
            {
                //string tmp = E.ToString();
            }
            finally
            {
                if (!IsComplete)
                    _state = EDownloadState.Stop;
                //Close the input stream
                stream?.Close();
            }
        }

        private void WriteFileProcess(StreamWriter streamWriter)
        {
            try
            {
                streamWriter.AutoFlush = true;
                Stream baseStream = streamWriter.BaseStream;
                //baseStream.Seek(0, SeekOrigin.End);
                long cacheBytes = 0;
                while (true)
                {
                    _orderManager.WaitOne();
                    DownloadPack? pack = GetDownloadData();
                    if (pack.HasValue)
                    {
                        DownloadPack tmp = pack.Value;
                        if (tmp.DataLength == 0)
                        {
                            ReleaseBuffer(tmp.Data);
                            continue;
                        }

                        baseStream.Write(tmp.Data, 0, tmp.DataLength);
                        ReleaseBuffer(tmp.Data);
                        cacheBytes += tmp.DataLength;
                        _currentPos = (baseStream.Length + baseStream.Position) / 2;
                        if (cacheBytes <= _bufferSize * 5) continue;
                        streamWriter.Flush();
                        cacheBytes = 0;
                    }
                    else
                    {
                        streamWriter.Flush();
                        if (!IsDownloading)
                            break;
                    }
                }
            }
            catch
            {
                streamWriter.Flush();
                if (!IsComplete)
                    _state = EDownloadState.Stop;
            }
            finally
            {
                if (_currentPos > 0 && _currentPos >= _finalPos)
                    _state = EDownloadState.Complete;
            }
        }

        private byte[] GetBuffer()
        {
            if (_bytePool.Count > 0)
            {
                byte[] tmp = null;
                lock (_bytePool)
                {
                    tmp = _bytePool.Pop();
                }

                //Parallel.For(0, _bufferSize, index => tmp[index] = 0);
                return tmp;
            }

            return new byte[_bufferSize];
        }

        private void ReleaseBuffer(byte[] buffer)
        {
            lock (_bytePool)
            {
                _bytePool.Push(buffer);
            }
        }

        private DownloadPack? GetDownloadData()
        {
            if (_dataPackQueue.Count > 0)
            {
                lock (_dataPackQueue)
                {
                    return _dataPackQueue.Dequeue();
                }
            }

            return null;
        }

        private void PushDownloadData(DownloadPack dataPack)
        {
            lock (_dataPackQueue)
            {
                _dataPackQueue.Enqueue(dataPack);
            }
        }
    }
}