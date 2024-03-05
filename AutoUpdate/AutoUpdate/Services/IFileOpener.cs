using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Services
{
    public interface IFileOpener
    {
        void OpenFile(byte[] data, string name);
    }
}
