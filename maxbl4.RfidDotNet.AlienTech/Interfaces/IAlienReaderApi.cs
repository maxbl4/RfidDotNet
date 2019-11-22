using System;
using System.Net;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;

namespace maxbl4.RfidDotNet.AlienTech.Interfaces
{
    public interface IAlienReaderApi
    {
        Task<string> ProgramEPC(string value = null);
        Task<int> ProgProtocol(int? value = null);
        Task<int> ProgAntenna(int? value = null);
        Task<int> ProgEPCDataIncCount(int? value = null);
        Task<int> ProgUserDataIncCount(int? value = null);
        Task<bool> ProgSingulate(bool? value = null);
        Task<IncrementDataMode> ProgEPCDataInc(IncrementDataMode? value = null);
        Task<IncrementDataMode> ProgUserDataInc(IncrementDataMode? value = null);
        
        Task<bool> TagListMillis(bool? value = null);
        Task<ReaderFunction> Function(ReaderFunction? value = null);
        Task<bool> TagStreamMode(bool? value = null);
        Task<IPEndPoint> TagStreamAddress(IPEndPoint value = null);
        Task<ListFormat> TagStreamFormat(ListFormat? value = null);
        Task<string> TagStreamCustomFormat(string value = null);
        Task<int> TagStreamKeepAliveTime(int? value = null);
        Task<int> TagStreamCountFilter(int? value = null);
        Task<TagStreamServer> TagStreamServer(TagStreamServer value = null);
        Task<bool> StreamHeader(bool? value = null);
        Task<string> HeartbeatNow();
        Task<int> HeartbeatTime(int? value = null);
        Task<bool> NotifyMode(bool? value = null);
        Task<int> AcqTime(int? value = null);
        Task<AcqG2Mode> AcqG2Mode(AcqG2Mode? value = null);
        Task<AcquireMode> AcquireMode(AcquireMode? value = null);
        Task<string> AcqMask(string value = null);
        Task<int> AcqG2Cycles(int? value = null);
        Task<int> AcqG2Count(int? value = null);
        Task<int> AcqG2Q(int? value = null);
        Task<int> AcqG2QMax(int? value = null);
        Task<int> AcqG2Select(int? value = null);
        Task<int> AcqG2Session(int? value = null);
        Task<string> AcqG2Mask(string value = null);
        Task<string> AcqG2MaskAction(string value = null);
        Task<string> AcqG2MaskAntenna(string value = null);
        Task<AcqG2SL> AcqG2SL(AcqG2SL? value = null);
        Task<string> SpeedFilter(string value = null);
        Task<string> RSSIFilter(string value = null);
        Task<string> ReaderName();
        Task<string> ReaderType();
        Task<string> ReaderVersion();
        Task<string> DSPVersion();
        Task<int> ReaderNumber(int? value = null);
        Task<string> Uptime();
        Task<int> MaxAntenna();
        Task<string> AntennaIO(string value = null);
        Task<string> AntennaStatus();
        Task<int> RFAttenuation(int? value = null);
        Task<int> RFLevel(int? value = null);
        Task<RFModulation> RFModulation(RFModulation? value = null);
        Task<string> Reboot();
        Task<string> MACAddress();
        Task<string> Hostname();
        Task<int> TimeZone(int? value = null);
        Task<int> TagDataFormatGroupSize(int? value = null);
        Task<int> TagFormatAntennaBase(int? value = null);
        Task<int> PersistTime(int? value = null);
        Task<bool> TagListAntennaCombine(bool? value = null);
        Task<bool> AutoMode(bool? value = null);
        Task<AutoAction> AutoAction(AutoAction? value = null);
        Task<int> AutoStopTimer(int? value = null);
        Task<string> AutoModeReset();
        Task<AutoModeType> AutoModeType(AutoModeType? value = null);
        Task<int> AutoSeekTimer(int? value = null);
        Task<int> AutoSeekPause(int? value = null);
        Task<int> AutoSeekRFLevel(int? value = null);
        Task<ListFormat> TagListFormat(ListFormat? value = null);
        Task<string> TagListCustomFormat(string value = null);
        Task<string> AntennaSequence(string value = null);
        Task<string> Clear();
        Task<T> GetProperty<T>(string name);
        Task<T> SetProperty<T>(string name, object value);
        Task<string> Save();
        Task<string> TagList();
        Task<bool> AcqG2AntennaCombine(bool? value = null);
        Task<string> Command(string command);
        Task<DateTime> Time(DateTime? value = null);
    }
}