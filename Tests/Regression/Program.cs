using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Downloading;
using LabFusion.Downloading.ModIO;
using System.IO.Compression;
using System.Buffers.Binary;
using LabFusion.Network.Proxy;

int failures = 0;
foreach (object input in new object[] { 42, 42u, (short)-42, (ushort)42, -42L, 42UL, true, (byte)7, (sbyte)-7, 2.5d, "control", 1.25f })
{
    using var writer = NetWriter.Create();
    object source = input;
    writer.SerializeValue(ref source);
    using var reader = NetReader.Create(writer.Buffer.ToArray());
    object result = null;
    try
    {
        reader.SerializeValue(ref result);
        Check(Equals(input, result), $"Object round trip: {input.GetType().Name}");
    }
    catch (Exception error)
    {
        Check(false, $"Object round trip: {input.GetType().Name}: {error.GetType().Name}");
    }
}

using (var writer = NetWriter.Create())
{
    object source = null;
    writer.SerializeValue(ref source);
    using var reader = NetReader.Create(writer.Buffer.ToArray());
    object result = "previous value";
    reader.SerializeValue(ref result);
    Check(result is null, "Null replaces an existing object");
}

var ids = new EntityIDList<object>();
ids.AddEntity(ushort.MaxValue, new object());
ids.ReserveID(ushort.MaxValue - 2);
ushort allocated = ids.AllocateNewID();
Check(!ids.HasEntity(allocated) && !ids.IsReserved(allocated), $"Allocation avoids occupied IDs with lower IDs free: returned {allocated}");

byte[] invalidCount = new byte[4];
BinaryPrimitives.WriteInt32BigEndian(invalidCount, 250000);
using (var reader = NetReader.Create(invalidCount))
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    try { reader.ReadStrings(); }
    catch (IndexOutOfRangeException) { }
    long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
    Check(allocatedBytes < 65536, $"Reject truncated string array before allocating: {allocatedBytes} bytes allocated");
}
using (var reader = NetReader.Create(invalidCount))
{
    Dictionary<string, string> metadata = null;
    long before = GC.GetAllocatedBytesForCurrentThread();
    try { reader.SerializeValue(ref metadata); }
    catch (IndexOutOfRangeException) { }
    long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
    Check(allocatedBytes < 65536, $"Reject truncated metadata before allocating: {allocatedBytes} bytes allocated");
}

foreach (bool nested in new[] { false, true })
{
    UnityEngine.Application.persistentDataPath = Path.Combine(AppContext.BaseDirectory, "pallet-" + Guid.NewGuid().ToString("N"));
    ModDownloadManager.ValidateDirectories();
    Directory.CreateDirectory(ModDownloadManager.ModsPath);
    string zipPath = Path.Combine(ModDownloadManager.DownloadPath, "m1f1.zip");
    using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
    using (var entry = new StreamWriter(zip.CreateEntry(nested ? "example/test.pallet.json" : "test.pallet.json").Open()))
        entry.Write("{}");
    bool scheduledCallback = false;
    ModForklift.Scheduled = 0;
    try
    {
        ModDownloadManager.LoadPalletFromZip(zipPath, new ModIOFile(1, 1), false, () => scheduledCallback = true);
        Check(scheduledCallback && ModForklift.Scheduled == 1, $"Pallet install: {(nested ? "nested" : "flat")}");
    }
    catch (Exception error)
    {
        Check(false, $"Pallet install: {(nested ? "nested" : "flat")}: {error.GetType().Name}, callback={scheduledCallback}");
    }
    finally
    {
        Directory.Delete(UnityEngine.Application.persistentDataPath, true);
    }
}

foreach (var length in new[] { -1, int.MaxValue, 250000 })
{
    byte[] payload = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(payload, length);
    using var reader = NetReader.Create(payload);
    CheckThrows<IndexOutOfRangeException>(() => reader.ReadStrings(), $"Reject string count {length}");
    reader.Position = 0;
    List<string> list = null;
    CheckThrows<IndexOutOfRangeException>(() => reader.SerializeValue(ref list), $"Reject list count {length}");
    reader.Position = 0;
    Dictionary<string, string> map = null;
    CheckThrows<IndexOutOfRangeException>(() => reader.SerializeValue(ref map), $"Reject dictionary count {length}");
    reader.Position = 0;
    CheckThrows<IndexOutOfRangeException>(() => reader.ReadBytes(), $"Reject byte count {length}");
}

using (var writer = NetWriter.Create())
{
    var source = new Dictionary<string, string> { ["empty"] = "", ["null"] = null, ["value"] = "hello" };
    writer.SerializeValue(ref source);
    using var reader = NetReader.Create(writer.Buffer.ToArray());
    Dictionary<string, string> result = null;
    reader.SerializeValue(ref result);
    Check(source.Count == result.Count && source.All(pair => result[pair.Key] == pair.Value), "Valid metadata round trip");
}

var fullIDs = new EntityIDList<object>();
for (int i = 0; i <= ushort.MaxValue; i++) fullIDs.ReservedIDs.Add((ushort)i);
CheckThrows<InvalidOperationException>(() => fullIDs.AllocateNewID(), "Entity ID exhaustion fails explicitly");
fullIDs.Unreserve(17);
Check(fullIDs.AllocateNewID() == 17, "Reuses the only free ID after exhaustion");

var installRoot = Path.Combine(Path.GetTempPath(), "fusion-update-test-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(installRoot);
try
{
    var installed = Path.Combine(installRoot, "LabFusion.dll");
    var original = new byte[] { 1, 2, 3, 4 };
    File.WriteAllBytes(installed, original);
    CheckThrows<BadImageFormatException>(() => LabFusionUpdater.Updater.InstallMod(new MemoryStream(new byte[100]), installed), "Invalid update rejected");
    Check(File.ReadAllBytes(installed).SequenceEqual(original), "Invalid update preserves installed file");
    CheckThrows<IOException>(() => LabFusionUpdater.Updater.InstallMod(new InterruptedStream(), installed), "Interrupted update rejected");
    Check(File.ReadAllBytes(installed).SequenceEqual(original), "Interrupted update preserves installed file");
    using var otherAssembly = File.OpenRead(typeof(EntityIDList<>).Assembly.Location);
    CheckThrows<InvalidDataException>(() => LabFusionUpdater.Updater.InstallMod(otherAssembly, installed), "Wrong assembly rejected");
    Check(Directory.GetFiles(installRoot).Length == 1, "Failed update removes staging files");
}
finally
{
    Directory.Delete(installRoot, true);
}

byte[] download = Enumerable.Range(0, 100000).Select(i => (byte)i).ToArray();
using (var content = new UnknownLengthContent(download))
using (var destination = new MemoryStream())
{
    Check(content.Headers.ContentLength is null, "Streaming fixture has no Content-Length");
    await ModIODownloader.CopyDownload(content, destination, download.Length, null);
    Check(destination.ToArray().SequenceEqual(download), "Downloads content with unknown length");
}
using (var content = new UnknownLengthContent(download))
using (var destination = new MemoryStream())
{
    CheckThrows<InvalidDataException>(() => ModIODownloader.CopyDownload(content, destination, 90000, null).GetAwaiter().GetResult(), "Streaming size limit enforced");
    Check(destination.Length <= 90000, "Does not write beyond size limit");
}
using (var content = new ByteArrayContent(new byte[10]))
using (var destination = new MemoryStream())
{
    content.Headers.ContentLength = 20;
    CheckThrows<InvalidDataException>(() => ModIODownloader.CopyDownload(content, destination, null, null).GetAwaiter().GetResult(), "Truncated download rejected");
}

var layer = new ProxyNetworkLayer();
var lobbyManager = new ProxyLobbyManager(layer);
var matchmaker = new ProxyMatchmaker(lobbyManager);
int lobbyCallbacks = 0;
matchmaker.RequestLobbies(info => lobbyCallbacks++);
matchmaker.RequestLobbies(info => lobbyCallbacks++);
Check(lobbyCallbacks == 2, "Lobby ID timeout completes callback and allows retry");

int requests = 0;
layer.OnSend = () =>
{
    if (requests++ % 2 == 0)
    {
        lobbyManager.HandleLobbyMessage(MessageTypes.LobbyIDs, new LiteNetLib.NetPacketReader(1u, 42UL));
    }
};
matchmaker.RequestLobbies(info => lobbyCallbacks++);
matchmaker.RequestLobbies(info => lobbyCallbacks++);
Check(lobbyCallbacks == 4, "Metadata timeout completes callback and allows retry");

layer.OnSend = null;
var oldRequest = lobbyManager.RequestLobbyIDs(new());
var newRequest = lobbyManager.RequestLobbyIDs(new());
Check(oldRequest.IsCanceled && !newRequest.IsCompleted, "Replacing lobby request cancels its previous task");
lobbyManager.CancelLobbyIDs(oldRequest);
Check(!newRequest.IsCompleted, "Old request cleanup preserves replacement");
lobbyManager.CancelLobbyIDs(newRequest);
Check(newRequest.IsCanceled, "Lobby cleanup cancels pending task");

var audio = new LabFusion.Audio.AudioStreamFilter(IntPtr.Zero);
for (int i = 0; i < LabFusion.Audio.AudioStreamFilter.QueueCapacity + 100; i++) audio.Enqueue(0.5f);
Check(audio.BufferedSamples == LabFusion.Audio.AudioStreamFilter.QueueCapacity, "Voice queue stays bounded");
using (var samples = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float>(new float[] { 1, 1, 1, 1 }))
{
    audio.ProcessAudioFilter(samples, 2);
    Check(samples.Values.All(value => value == 0.5f), "Voice samples reach both channels");
}
await Task.WhenAll(
    Task.Run(() => { for (int i = 0; i < 100000; i++) audio.Enqueue(0.5f); }),
    Task.Run(() => { for (int i = 0; i < 10000; i++) audio.ClearValues(); }),
    Task.Run(() =>
    {
        using var samples = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float>(new float[512]);
        for (int i = 0; i < 1000; i++) audio.ProcessAudioFilter(samples, 2);
    }));
Check(audio.BufferedSamples >= 0 && audio.BufferedSamples <= LabFusion.Audio.AudioStreamFilter.QueueCapacity, "Concurrent voice enqueue, clear, and playback keep the queue valid");

foreach (bool throwInCallback in new[] { false, true })
{
    var palletRoot = Path.Combine(Path.GetTempPath(), "fusion-pallet-test-" + Guid.NewGuid().ToString("N"));
    UnityEngine.Application.persistentDataPath = palletRoot;
    ModDownloadManager.ValidateDirectories();
    var invalidZip = Path.Combine(ModDownloadManager.DownloadPath, "invalid.zip");
    File.WriteAllText(invalidZip, "invalid zip");
    int failed = 0, scheduled = 0;
    try
    {
        ModDownloadManager.LoadPalletFromZip(invalidZip, new ModIOFile(1, 1), false,
            () => scheduled++, info =>
            {
                if (info.Result == ModResult.FAILED) failed++;
                if (throwInCallback) throw new InvalidOperationException("Test callback failure.");
            });
    }
    catch (InvalidOperationException) when (throwInCallback) { }
    Check(failed == 1 && scheduled == 1, $"Pallet failure completes callbacks (throwing callback: {throwInCallback})");
    Check(!Directory.EnumerateDirectories(ModDownloadManager.ExportPath).Any(), "Failed extraction is cleaned up");
    Directory.Delete(palletRoot, true);
}

Console.WriteLine($"Failures: {failures}");
Environment.ExitCode = failures == 0 ? 0 : 1;

void CheckThrows<T>(Action action, string label) where T : Exception
{
    try
    {
        action();
        Check(false, label);
    }
    catch (T)
    {
        Check(true, label);
    }
}

void Check(bool passed, string label)
{
    Console.WriteLine($"{(passed ? "PASS" : "FAIL")}: {label}");
    if (!passed) failures++;
}

sealed class InterruptedStream : MemoryStream
{
    public override void CopyTo(Stream destination, int bufferSize)
    {
        destination.WriteByte(42);
        throw new IOException("Interrupted test stream.");
    }
}

sealed class UnknownLengthContent : HttpContent
{
    private readonly byte[] _data;
    public UnknownLengthContent(byte[] data) => _data = data;
    protected override bool TryComputeLength(out long length) { length = 0; return false; }
    protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context) => stream.WriteAsync(_data, 0, _data.Length);
    protected override Task<Stream> CreateContentReadStreamAsync() => Task.FromResult<Stream>(new MemoryStream(_data));
}
