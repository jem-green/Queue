using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Queue
{
    class PersistentQueue<T> : IEnumerator<T>
    {
        // Create a queue that is actually a file
        // Need to consider locking as might hsve problems reading in writing simultaniously
        // Start simple and just have it as a string queue
        // assume that the file is stored with the dll/exe
        //
        // Header
        // ------
        //
        // 00 - unsigned int16 - number of elements _size
        // 00 - unsigned int16 - current element counter _count
        // 00 - unsigned int16 - pointer to current element _pointer
        //
        // Data
        // ----
        //
        // - Depending on data type but for string
        // 00 - leb128 - Length of element handled by the binary writer and reader in LEB128 format
        // bytes - string
        // ...
        // 00 - leb128 - Length of element handled by the binary writer and reader in LEB128 format
        // bytes - string
        //
        //


        #region Fields

        string _path = "";
        string _name = "PersistentQueue";
        readonly object _lockObject = new Object();
        UInt16 _size;                                   // number of elements
        UInt16 _count;                                  // current element pointer
        UInt16 _pointer;                                // pointer to the end of the queue
        UInt16 _data = 6;                               // pointer to start of data

        int _cursor;
        private bool disposedValue;

        #endregion
        #region Constructors

        public PersistentQueue()
        {
            Open(_path, _name, false);
        }

        public PersistentQueue(bool reset)
        {
            Open(_path, _name, reset);
        }

        public PersistentQueue(string filename)
        {
            _name = filename;
            Open(_path, _name, false);
        }

        public PersistentQueue(string path, string filename)
        {
            _name = filename;
            _path = path;
            Open(_path, _name, false);
        }
        public PersistentQueue(string path, string filename, bool reset)
        {
            _name = filename;
            _path = path;
            Open(_path, _name, reset);
        }

        #endregion
        #region Proprties

        public int Count
        {
            get
            {
                return (_size - _count);
            }
        }

        public string Path
        {
            get
            {
                return (_path);
            }
            set
            {
                _path = value;
            }
        }

        public string Name
        {
            get
            {
                return (_name);
            }
            set
            {
                _name = value;
            }
        }




        #endregion
        #region Methods

        /// <summary>
        /// Clear the Queue
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                Reset(_path, _name);
            }
        }

        /// <summary>
        /// Read the data from the top of the Queue
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            object data;

            lock (_lockObject)
            {
                Type typeParameterType = typeof(T);
                string filenamePath = System.IO.Path.Combine(_path, _name + ".bin");

                // Check 

                if ((_size - _count) > 0)
                {

                    // Find the data

                    BinaryReader binaryReader = new BinaryReader(new FileStream(filenamePath, FileMode.Open));
                    binaryReader.BaseStream.Seek(_data + _pointer, SeekOrigin.Begin);    // Move to position of the current item

                    if (typeParameterType == typeof(string))
                    {
                        data = binaryReader.ReadString();
                        //_pointer = Convert.ToUInt16(_pointer + 1 + data.ToString().Length);
                    }
                    else
                    {
                        data = default(T);
                    }
                    binaryReader.Close();

                }
                else
                {
                    throw new InvalidOperationException("Queue empty.");
                }
            }

            return ((T)Convert.ChangeType(data, typeof(T)));
        }

        /// <summary>
        /// Add data to the Queue
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(T data)
        {
            lock (_lockObject)
            {
                Type typeParameterType = typeof(T);

                string filenamePath = System.IO.Path.Combine(_path, _name + ".bin");
                BinaryWriter binaryWriter = new BinaryWriter(new FileStream(filenamePath, FileMode.OpenOrCreate));
                binaryWriter.Seek(0, SeekOrigin.Begin); // Move to start of the file
                _size++;
                binaryWriter.Write(_size);  // Write the size
                // _pointer and _count are not updated
                binaryWriter.Close();

                // Appending will only work if the file is deleted and the updates start again
                // Not sure if this is the best approach

                binaryWriter = new BinaryWriter(new FileStream(filenamePath, FileMode.Append));

                if (typeParameterType == typeof(string))
                {
                    string s = Convert.ToString(data);
                    binaryWriter.Write(s);
                }
                binaryWriter.Close();
            }
        }
        
        /// <summary>
        /// Remove data to the Queue
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            object data;

            lock (_lockObject)
            {
                Type typeParameterType = typeof(T);
                string filenamePath = System.IO.Path.Combine(_path, _name + ".bin");

                // Check 

                if ((_size - _count) > 0)
                {
                    // Find the data

                    BinaryReader binaryReader = new BinaryReader(new FileStream(filenamePath, FileMode.Open));
                    binaryReader.BaseStream.Seek(_data + _pointer, SeekOrigin.Begin);    // Move to position of the current

                    if (typeParameterType == typeof(string))
                    {
                        data = binaryReader.ReadString();
                        _pointer = Convert.ToUInt16(_pointer + 1 + data.ToString().Length);
                    }
                    else
                    {
                        data = default(T);
                    }
                    binaryReader.Close();

                    _count = (UInt16)(_count + 1);

                    // Check 
                    if (_count < _size)
                    {
                        BinaryWriter binaryWriter = new BinaryWriter(new FileStream(filenamePath, FileMode.OpenOrCreate));
                        binaryWriter.Seek(2, SeekOrigin.Begin); // Move to start of the file and skip size
                        binaryWriter.Write(_count);  // Write the new count
                        binaryWriter.Write(_pointer);  // Write the new pointer
                        binaryWriter.Close();
                    }
                    else
                    {
                        Clear();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Queue empty.");
                }
            }

            return ((T)Convert.ChangeType(data, typeof(T)));
        }


        bool IEnumerator.MoveNext()
        {
            bool moved = false;
            if (_cursor < _size)
            {
                moved = true;
            }
            return (moved);
        }

        void IEnumerator.Reset()
        {
            _cursor = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                if ((_cursor < 0) || (_cursor == _size))
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    return (Read(_path, _name, _cursor));
                }
            }
        }

        T IEnumerator<T>.Current
        {
            get
            {
                if ((_cursor < 0) || (_cursor == _size))
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    return ((T)Convert.ChangeType(Read(_path, _name, _cursor), typeof(T)));
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            for (int cursor=_count; cursor < _size; cursor++)
            {
                // Return the current element and then on next function call 
                // resume from next element rather than starting all over again;
                yield return (Read(_path, _name, cursor));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PersistentQueue()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
        #region Private

        private void Open(string path, string filename, bool reset)
        {
            string filenamePath = System.IO.Path.Combine(path, filename + ".bin");

            if ((File.Exists(filenamePath) == true) && (reset == false))
            {
                BinaryReader binaryReader = new BinaryReader(new FileStream(filenamePath, FileMode.Open));
                _size = binaryReader.ReadUInt16();
                _count = binaryReader.ReadUInt16();
                _pointer = binaryReader.ReadUInt16();
                _cursor = -1;
                binaryReader.Close();
            }
            else
            {
                _cursor = -1;
                File.Delete(filenamePath);
                Reset(path, filename);
            }
        }

        private void Reset(string path, string filename)
        {
            // Reset the file
            string filenamePath = System.IO.Path.Combine(path, filename + ".bin");
            BinaryWriter binaryWriter = new BinaryWriter(new FileStream(filenamePath, FileMode.OpenOrCreate));
            binaryWriter.Seek(0, SeekOrigin.Begin); // Move to start of the file
            _size = 0;
            _count = 0;
            _pointer = 0;                   // Start of the data
            binaryWriter.Write(_size);      // Write the new size
            binaryWriter.Write(_count);     // Write the new count
            binaryWriter.Write(_pointer);   // Write the new pointer
            binaryWriter.BaseStream.SetLength(6);
            binaryWriter.Close();
        }

        private object Read(string path, string filename, int index)
        {
            object data = null;

            lock (_lockObject)
            {
                Type typeParameterType = typeof(T);
                string filenamePath = System.IO.Path.Combine(path, filename + ".bin");

                // Check 

                if ((_size - _count) > 0)
                {
                    // Find the data

                    BinaryReader binaryReader = new BinaryReader(new FileStream(filenamePath, FileMode.Open));
                    binaryReader.BaseStream.Seek(_data + _pointer, SeekOrigin.Begin);    // Move to position of the current
                    for (int cursor = 0; cursor < (_size - _count); cursor++)
                    {

                        if (typeParameterType == typeof(string))
                        {
                            object temp = binaryReader.ReadString();
                            if (cursor == index)
                            {
                                data = temp;
                                break;
                            }
                        }
                    }
                    binaryReader.Close();
                }
            }
            return (data);
        }

        #endregion
    }
}
