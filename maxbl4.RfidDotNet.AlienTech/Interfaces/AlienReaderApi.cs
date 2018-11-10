using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using maxbl4.RfidDotNet.AlienTech.Enums;
using maxbl4.RfidDotNet.AlienTech.Ext;

namespace maxbl4.RfidDotNet.AlienTech.Interfaces
{
    public abstract class AlienReaderApi : IAlienReaderApi
    {
        public abstract Task<string> SendReceive(string command);

        public Task<bool> TagListMillis(bool? value = null)
        {
            return Property<bool>(value);
        }

        public Task<ReaderFunction> Function(ReaderFunction? value = null)
        {
            return Property<ReaderFunction>(value);
        }

        public Task<DateTimeOffset> Time(DateTimeOffset? value = null)
        {
            return Property<DateTimeOffset>(value);
        }

        public Task<bool> TagStreamMode(bool? value = null)
        {
            return Property<bool>(value);
        }

        public Task<IPEndPoint> TagStreamAddress(IPEndPoint value = null)
        {
            return Property<IPEndPoint>(value);
        }
        public Task<ListFormat> TagStreamFormat(ListFormat? value = null)
        {
            return Property<ListFormat>(value);
        }
        public Task<string> TagStreamCustomFormat(string value = null)
        {
            return Property<string>(value);
        }
        //TagStreamKeepAliveTime int 30
        public Task<int> TagStreamKeepAliveTime(int? value = null)
        {
            return Property<int>(value);
        }
        //TagStreamCountFilter int 0
        public Task<int> TagStreamCountFilter(int? value = null)
        {
            return Property<int>(value);
        }
        //TagStreamServer string 0 3333 any
        public async Task<TagStreamServer> TagStreamServer(TagStreamServer value = null)
        {
            return new TagStreamServer(await Property<string>(value?.ToString()));
        }
        //StreamHeader bool true
        public Task<bool> StreamHeader(bool? value = null)
        {
            return Property<bool>(value);
        }

        public Task<string> HeartbeatNow()
        {
            return Command<string>();
        }

        public Task<int> HeartbeatTime(int? value = null)
        {
            return Property<int>(value);
        }

        public Task<bool> NotifyMode(bool? value = null)
        {
            return Property<bool>(value);
        }
        
        public Task<int> ProgProtocol(int? value = null)
        {
            return Property<int>(value);
        }
        
        public Task<int> ProgAntenna(int? value = null)
        {
            return Property<int>(value);
        }

        public Task<string> ProgramEPC(string value = null)
        {
            return Property<string>(value);
        }
        
        public Task<int> ProgEPCDataIncCount(int? value = null)
        {
            return Property<int>(value);
        }
        
        public Task<int> ProgUserDataIncCount(int? value = null)
        {
            return Property<int>(value);
        }
        
        public Task<bool> ProgSingulate(bool? value = null)
        {
            return Property<bool>(value);
        }
        
        public Task<IncrementDataMode> ProgEPCDataInc(IncrementDataMode? value = null)
        {
            return Property<IncrementDataMode>(value);
        }
        
        public Task<IncrementDataMode> ProgUserDataInc(IncrementDataMode? value = null)
        {
            return Property<IncrementDataMode>(value);
        }

        //AcqTime int 0
        public Task<int> AcqTime(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2Mode enum 25|26
        public Task<AcqG2Mode> AcqG2Mode(AcqG2Mode? value = null)
        {
            return Property<AcqG2Mode>(value);
        }
        //AcquireMode Inventory|Global Scroll
        public Task<AcquireMode> AcquireMode(AcquireMode? value = null)
        {
            return Property<AcquireMode>(value);
        }
        //AcqMask string null
        public Task<string> AcqMask(string value = null)
        {
            return Property<string>(value);
        }
        //AcqG2Cycles int 1 [0:255]
        public Task<int> AcqG2Cycles(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2Count int 1 [0:255]
        public Task<int> AcqG2Count(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2Q int 3 [0:10]
        public Task<int> AcqG2Q(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2QMax int 12 [0:15]
        public Task<int> AcqG2QMax(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2Select int 1 [0:255]
        public Task<int> AcqG2Select(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2Session int 2 [0:3]
        public Task<int> AcqG2Session(int? value = null)
        {
            return Property<int>(value);
        }
        //AcqG2Mask string null
        public Task<string> AcqG2Mask(string value = null)
        {
            return Property<string>(value);
        }
        //AcqG2MaskAction string 
        public Task<string> AcqG2MaskAction(string value = null)
        {
            return Property<string>(value);
        }
        //AcqG2MaskAntenna string
        public Task<string> AcqG2MaskAntenna(string value = null)
        {
            return Property<string>(value);
        }
        //AcqG2SL all SL|nSL|All
        public Task<AcqG2SL> AcqG2SL(AcqG2SL? value = null)
        {
            return Property<AcqG2SL>(value);
        }
        //SpeedFilter string null
        public Task<string> SpeedFilter(string value = null)
        {
            return Property<string>(value);
        }
        //RSSIFilter string null
        public Task<string> RSSIFilter(string value = null)
        {
            return Property<string>(value);
        }
        //ReaderName
        public Task<string> ReaderName()
        {
            return Property<string>();
        }
        //ReaderType
        public Task<string> ReaderType()
        {
            return Property<string>();
        }
        //ReaderVersion
        public Task<string> ReaderVersion()
        {
            return Property<string>();
        }
        //DSPVersion
        public Task<string> DSPVersion()
        {
            return Property<string>();
        }
        //ReaderNumber int 0 [0:255]
        public Task<int> ReaderNumber(int? value = null)
        {
            return Property<int>(value);
        }
        //Uptime
        public Task<string> Uptime()
        {
            return Property<string>();
        }
        //MaxAntenna int 
        public Task<int> MaxAntenna()
        {
            return Property<int>();
        }
        //AntennaIO string
        public Task<string> AntennaIO(string value = null)
        {
            return Property<string>(value);
        }
        //AntennaStatus string
        public Task<string> AntennaStatus()
        {
            return Property<string>();
        }
        //RFAttenuation int 0 [0:150]
        public Task<int> RFAttenuation(int? value = null)
        {
            return Property<int>(value);
        }
        //RFLevel int 307 [157:307]
        public Task<int> RFLevel(int? value = null)
        {
            return Property<int>(value);
        }
        //RFModulation DRM|STD|HS|25M4|12M4|06M4|25FM0|12FM0|06FM0|25M2|12M2|06M2
        public Task<RFModulation> RFModulation(RFModulation? value = null)
        {
            return Property<RFModulation>(value);
        }
        //Reboot
        public Task<string> Reboot()
        {
            return Command<string>();
        }
        //MACAddress
        public Task<string> MACAddress()
        {
            return Property<string>();
        }
        //Hostname
        public Task<string> Hostname()
        {
            return Property<string>();
        }
        //TimeZone
        public Task<int> TimeZone(int? value = null)
        {
            return Property<int>(value);
        }
        //TagDataFormatGroupSize int 2 [0:2]
        public Task<int> TagDataFormatGroupSize(int? value = null)
        {
            return Property<int>(value);
        }
        //TagFormatAntennaBase int 0 [0:10]
        public Task<int> TagFormatAntennaBase(int? value = null)
        {
            return Property<int>(value);
        }
        //PersistTime int -1
        public Task<int> PersistTime(int? value = null)
        {
            return Property<int>(value);
        }
        //TagListAntennaCombine bool false
        public Task<bool> TagListAntennaCombine(bool? value = null)
        {
            return Property<bool>(value);
        }
        //TagListAntennaCombine bool false
        public Task<bool> AcqG2AntennaCombine(bool? value = null)
        {
            return Property<bool>(value);
        }
        //AutoMode bool false
        public Task<bool> AutoMode(bool? value = null)
        {
            return Property<bool>(value);
        }
        //AutoAction Acquire [None|Acquire|ProgramEPC|ProgramAndLockEPC|Erase|ProgramAlienImage|ProgramUser|ProgramAndLockUser
        public Task<AutoAction> AutoAction(AutoAction? value = null)
        {
            return Property<AutoAction>(value);
        }
        //AutoStopTimer int 1000
        public Task<int> AutoStopTimer(int? value = null)
        {
            return Property<int>(value);
        }
        //AutoModeReset
        public Task<string> AutoModeReset()
        {
            return Command<string>();
        }
        //AutoModeType Standard|Seek
        public Task<AutoModeType> AutoModeType(AutoModeType? value = null)
        {
            return Property<AutoModeType>(value);
        }
        //AutoSeekTimer int 500
        public Task<int> AutoSeekTimer(int? value = null)
        {
            return Property<int>(value);
        }
        //AutoSeekPause int 250
        public Task<int> AutoSeekPause(int? value = null)
        {
            return Property<int>(value);
        }
        //AutoSeekRFLevel
        public Task<int> AutoSeekRFLevel(int? value = null)
        {
            return Property<int>(value);
        }

        public Task<ListFormat> TagListFormat(ListFormat? value = null)
        {
            return Property<ListFormat>(value);
        }

        public Task<string> TagListCustomFormat(string value = null)
        {
            return Property<string>(value);
        }

        public Task<string> AntennaSequence(string value = null)
        {
            return Property<string>(value);
        }

        public Task<string> Clear()
        {
            return Command<string>();
        }

        public Task<T> GetProperty<T>(string name)
        {
            return Property<T>(null, true, name);
        }

        public Task<T> SetProperty<T>(string name, object value)
        {
            return Property<T>(value, true, name);
        }

        private async Task<T> Property<T>(object value = null, bool checkPrefix = true, [CallerMemberName]string name = null)
        {
            var stringValue = AlienValueConverter.ToAlienValueString(value);
            string response;
            if (string.IsNullOrEmpty(stringValue))
                response = await SendReceive($"{name}?");
            else
                response = await SendReceive($"{name} = {stringValue}");
            var responsePrefix = $"{name} = ";
            if (checkPrefix && !response.StartsWith(responsePrefix))
                throw new Exception($"Unexcpected response: {response}");
            if (checkPrefix)
                response = response.Substring(responsePrefix.Length);
            return AlienValueConverter.ToStrongType<T>(response.TrimEnd('\0', '\r', '\n'));
        }

        public Task<string> Command(string command)
        {
            return Command<string>(command);
        }

        private async Task<T> Command<T>([CallerMemberName] string name = null)
        {
            return AlienValueConverter.ToStrongType<T>(await SendReceive(name));
        }

        public Task<string> Save()
        {
            return Command<string>();
        }

        public Task<string> TagList()
        {
            return Property<string>(checkPrefix: false);
        }
    }
}