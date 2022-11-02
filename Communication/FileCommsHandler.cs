using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public class FileCommsHandler
    {
        private readonly ConversionHandler _conversionHandler;
        private readonly FileHandler _fileHandler;
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly NetworkHelper networkHelper;
        private readonly NetworkStream networkStream;
        public FileCommsHandler(NetworkHelper myNetworkHelper)
        {
            _conversionHandler = new ConversionHandler();
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            networkHelper = myNetworkHelper;
        }

        public bool ValidatePath(string path) {

            return (_fileHandler.FileExists(path));

        }

        public void SendFile(string path)
        {
            if (ValidatePath(path))
            {
                var fileName = _fileHandler.GetFileName(path);
                // ---> Enviar el largo del nombre del archivo
                networkHelper.Send(_conversionHandler.ConvertIntToBytes(fileName.Length));
                // ---> Enviar el nombre del archivo
                networkHelper.Send(_conversionHandler.ConvertStringToBytes(fileName));

                // ---> Obtener el tamaño del archivo
                long fileSize = _fileHandler.GetFileSize(path);
                // ---> Enviar el tamaño del archivo
                var convertedFileSize = _conversionHandler.ConvertLongToBytes(fileSize);
                networkHelper.Send(convertedFileSize);
                // ---> Enviar el archivo (pero con file stream)
                SendFileWithStream(fileSize, path);
                
            }
            else
            {
                throw new Exception("File Does Not Exist");
            }
        }

        public string ReceiveFile()
        {
            // ---> Recibir el largo del nombre del archivo
            int fileNameSize = _conversionHandler.ConvertBytesToInt(
                networkHelper.ReceiveAsync(Protocol.FixedDataSize).Result);
            // ---> Recibir el nombre del archivo
            string fileName = _conversionHandler.ConvertBytesToString(networkHelper.ReceiveAsync(fileNameSize).Result);
            // ---> Recibir el largo del archivo
            long fileSize = _conversionHandler.ConvertBytesToLong(
                networkHelper.ReceiveAsync(Protocol.FixedFileSize).Result);
            // ---> Recibir el archivo
            ReceiveFileWithStreams(fileSize, fileName);
            
            return fileName;
        }

        private void SendFileWithStream(long fileSize, string path)
        {
            long fileParts = Protocol.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _fileStreamHandler.Read(path, offset, Protocol.MaxPacketSize);
                    offset += Protocol.MaxPacketSize;
                }

                networkHelper.Send(data);
                currentPart++;
            }
        }

        private void ReceiveFileWithStreams(long fileSize, string fileName)
        {
            long fileParts = Protocol.CalculateFileParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == fileParts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = networkHelper.ReceiveAsync(lastPartSize).Result;
                    offset += lastPartSize;
                }
                else
                {
                    data = networkHelper.ReceiveAsync(Protocol.MaxPacketSize).Result;
                    offset += Protocol.MaxPacketSize;
                }
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
    }
}

