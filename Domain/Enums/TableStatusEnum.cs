using System.ComponentModel;

public enum TableStatusEnum
{
    [Description("Trống")]
    Available = 0,
    
    [Description("Đang phục vụ")]
    InUse = 1,
    
    [Description("Đã đặt trước")]
    Reserved = 2,
    
    [Description("Không khả dụng")]
    Unavailable = 3
} 