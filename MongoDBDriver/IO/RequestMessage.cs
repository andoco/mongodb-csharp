using System;
using System.Diagnostics;
using System.IO;

using MongoDB.Driver.Bson;

namespace MongoDB.Driver.IO
{
    /// <summary>
    /// Description of Message.
    /// </summary>
    public abstract class RequestMessage : Message
    {
        
        public RequestMessage()
        {}
        
        public void Write (Stream stream){
            MessageHeader header = this.Header;
            BufferedStream bstream = new BufferedStream(stream);
            BinaryWriter writer = new BinaryWriter(bstream);
            BsonWriter bwriter = new BsonWriter(bstream);
            
            Header.MessageLength += this.CalculateBodySize(bwriter);
            
            writer.Write(header.MessageLength);
            writer.Write(header.RequestId);
            writer.Write(header.ResponseTo);
            writer.Write((int)header.OpCode);
            writer.Flush();
            WriteBody(bwriter);
            bwriter.Flush();
        }
        
        protected abstract void WriteBody(BsonWriter writer);
        
        protected abstract int CalculateBodySize(BsonWriter writer);

    }
}
