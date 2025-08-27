// <copyright file="DeviceInfo.cs" company="Microsoft">Copyright (c) Microsoft. All rights reserved.</copyright>

namespace Microsoft.Commerce.Payments.PXService.Accessors.LegacyCommerceService.DataModel
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = NamespaceConstants.Namespace)]
    public class DeviceInfo
    {
        /// <summary>
        /// From the TCP/IP session - the IP V4 or IP V6 address of the end-user initiating the request, or the IP address of the facilitating ISP.
        /// Partners need to record the closest-to-home IP address of the user that they can get from the original request.
        /// Example: "255.255.255.255"
        /// </summary>
        [DataMember]
        public string IPAddress { get; set; }

        /// <summary>
        /// The user's device type. Device types are: Xbox", Xbox360, and "Browser" (for all web browsers).
        /// Example: "Browser", "Xbox", "Xbox360"
        /// </summary>
        [DataMember]
        public string DeviceType { get; set; }

        /// <summary>
        /// The serial number of the device from which the user is initiating the request. For HTTP-initiated requests on PCs, this field should be left blank.
        /// For Xbox-initiated requests, this field should contain the unique ID of the Xbox device.
        /// Example:
        ///   For Xbox1: 12 digit Console Serial Number: "NNNNNNCYWWOO"
        ///   For Xbox360: 12 digit Console ID
        /// </summary>
        [DataMember]
        public string DeviceId { get; set; }

        /// <summary>
        /// Client name
        /// </summary>
        [DataMember]
        public string ClientName { get; set; }
    }
}
