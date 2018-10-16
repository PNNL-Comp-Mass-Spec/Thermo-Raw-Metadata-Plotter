using ThermoRawMetadataReader;

namespace RawReaderMetadata
{
    public class InstanceCreator : IInstanceCreator
    {
        public IMetadataReader CreateInstance(string rawFilePath)
        {
            return new RawReaderMetadata(rawFilePath);
        }
    }
}
