namespace ThermoRawMetadataReader
{
    public interface IInstanceCreator
    {
        IMetadataReader CreateInstance(string rawFilePath);
    }
}
