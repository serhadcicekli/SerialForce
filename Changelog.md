# Changelog

# Version 1.01

- SFData renamed to SFObject.

- Added serializable SFFloat16, SFFloat32 and SFFloat64 object types for 16-bit, 32-bit and 64-bit floating-point numbers.

- The text-based type block in safe serialized data has been replaced by a hash-based type block. Therefore, serializable SFTypeResolver class was added, which contains object types and their hashes to detect the type of the securely serialized object.

- The SFSafeSerializator class, which was used to securely serialize SFData-based serializable objects with a type block containing the type of that object and a hash block to keep the data intact, has been removed. The object can now be safely serialized with these methods:

For serialization:
```csharp
byte[] serializedData = SFObject.Serialized;
```
For deserialization:
```csharp
SFObject.Serialized = serializedData;
```
# Version 1.0
- Initial version of SerialForce.
