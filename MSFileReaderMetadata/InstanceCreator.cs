using ThermoRawMetadataReader;

namespace MSFileReaderMetadata
{
    public class InstanceCreator : IInstanceCreator
    {
        public IMetadataReader CreateInstance(string rawFilePath)
        {
            return new MSFileReaderMetadata(rawFilePath);
        }
    }
}
