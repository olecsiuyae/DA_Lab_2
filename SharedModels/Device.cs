using System;
using System.Collections.Generic;

namespace SharedModels
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid DeviceTypeId { get; set; }
        public DeviceType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DeviceType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<DevicePort> Ports { get; set; }
    }

    public class DevicePort
    {
        public Guid Id { get; set; }
        public PortType PortType { get; set; }
        public Guid DeviceTypeId { get; set; }
        public DeviceType DeviceType { get; set; }
    }

    public enum PortType
    {
        COM,
        USB,
        LPT
    }
} 