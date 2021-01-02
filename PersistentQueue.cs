using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Queue
{
    class PersistentQueue<T>
    {
        // Create a queue that is actually a file
        // Need to consider locking as might hsve problems reading in writing simultaniously
        // Start simple and just have it as a string queue
        // assume that the file is stored with the class

        // Try this
        //
        // 0000 - unsigned int - number of elements
        // 0000 - unsigned int - current element conter
        // 0000 - unsigned int - pointer to current element
        // - Depending on data type but for string
        // 00 - unsigned int - Length of element handled by the binary writer and reader
        // bytes - string
        // ...
        // 00 - unsigned int - Length of element handled by the binary writer and reader
        // bytes - string
        //
        //


        #region Variables

        string _path = "";
        string _name = "PersistentQueue";
        readonly object _lockObject = new Object();
        UInt16 _size;
        UInt16 _count;
        UInt16 _pointer;

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

        public void Clear()
        {
            lock (_lockObject)
            {
                Reset(_path, _name);
            }
        }

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
                    binaryReader.BaseStream.Seek(_pointer, SeekOrigin.Begin);    // Move to position of the current

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
                    binaryReader.BaseStream.Seek(_pointer, SeekOrigin.Begin);    // Move to position of the current

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
                        binaryWriter.Seek(2, SeekOrigin.Begin); // Move to start of the file
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
                binaryReader.Close();
            }
            else
            {
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
            _pointer = 6;   // Start of the data
            binaryWriter.Write(_size);  // Write the new size
            binaryWriter.Write(_count);  // Write the new count
            binaryWriter.Write(_pointer);  // Write the new pointer
            binaryWriter.BaseStream.SetLength(6);
            binaryWriter.Close();
        }

        #endregion

    }
}
