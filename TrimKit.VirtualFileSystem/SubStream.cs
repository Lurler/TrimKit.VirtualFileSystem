namespace TrimKit.VirtualFileSystem;

internal class SubStream : Stream
{
    private readonly Stream baseStream;

    private readonly long length;

    private readonly long streamOffset;

    private long position;

    public SubStream(Stream stream, long offset, long length)
    {
        this.baseStream = stream;
        this.streamOffset = offset;
        this.length = length;
        this.position = 0;
        this.baseStream.Position = offset;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => this.length;

    public override long Position
    {
        get => this.position;
        set
        {
            if (value < 0
                || value > this.length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            this.position = value;
            this.baseStream.Position = this.streamOffset + value;
        }
    }

    public override void Flush()
    {
        this.baseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var maxCount = (int)Math.Min(count, this.length - this.position);
        if (maxCount <= 0)
        {
            return 0;
        }

        var bytesRead = this.baseStream.Read(buffer, offset, maxCount);
        this.position += bytesRead;
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPosition = origin switch
        {
            SeekOrigin.Begin   => offset,
            SeekOrigin.Current => this.position + offset,
            SeekOrigin.End     => this.length + offset,
            _                  => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };

        if (newPosition < 0
            || newPosition > this.length)
        {
            throw new IOException("Seek position is out of range");
        }

        this.Position = newPosition;
        return this.position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.baseStream?.Dispose();
        }

        base.Dispose(disposing);
    }
}