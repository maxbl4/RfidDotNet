namespace maxbl4.RfidDotNet.GenericSerial.Model
{
    public enum ResponseStatusCode: byte
    {
        Success = 0x0,
        InventoryComplete = 0x1,
        InventoryTimeout = 0x2,
        InventoryMoreFramesPending = 0x3,
        InventoryBufferOverflow = 0x4,
        InventoryStatisticsDelivery = 0x26,
        
        InvalidPassword = 0x5,
        TagKillFailed = 0x9,
        TagKillAllZeroPassword = 0xA,
        CommandIsNotSupportedByTag = 0xB,
        AllZeroAccessPassword = 0xC,
        ReadProtectionFailedToEnable = 0xD,
        ReadProtectionFailedToUnlockTag = 0xE,
        
        //0xEE
        HeartBeatDelivered = 0x28,
        
        CommandExecutionFailed = 0xF9,
        OperationFailed = 0xFA,
        NoOperableTagsFound = 0xFB,
        /// <summary>
        /// Data should contain one byte error code from tag
        /// </summary>
        ErrorReturnFromTag = 0xFC,
        CommandFrameLengthError = 0xFD,
        IllegalCommand = 0xFE,
        CommandParametersError = 0xFF,
        
    }
}