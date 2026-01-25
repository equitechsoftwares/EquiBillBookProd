using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace EquiBillBook.Helpers
{
    public static class SocketIoHelper
    {
        private static readonly string SocketIoServiceUrl = WebConfigurationManager.AppSettings["SocketIoServiceUrl"] ?? "http://localhost:3000";

        /// <summary>
        /// Emit event to Socket.IO service
        /// </summary>
        public static async Task EmitEvent(string eventName, object data, long? companyId = null, long? branchId = null, long? stationId = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var payload = new
                    {
                        eventName = eventName,
                        data = data,
                        companyId = companyId,
                        branchId = branchId,
                        stationId = stationId
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{SocketIoServiceUrl}/api/emit", content);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - Socket.IO is not critical for core functionality
                System.Diagnostics.Debug.WriteLine($"Socket.IO emit error: {ex.Message}");
            }
        }

        /// <summary>
        /// Emit KOT created event
        /// </summary>
        public static async Task EmitKotCreated(object kotData, long companyId, long branchId, long? stationId = null)
        {
            await EmitEvent("kot-created", kotData, companyId, branchId, stationId);
        }

        /// <summary>
        /// Emit KOT status updated event
        /// </summary>
        public static async Task EmitKotStatusUpdated(object kotData, long companyId, long branchId, long? stationId = null)
        {
            await EmitEvent("kot-status-updated", kotData, companyId, branchId, stationId);
        }

        /// <summary>
        /// Emit KOT linked to sales event
        /// </summary>
        public static async Task EmitKotLinkedToSales(object linkData, long companyId, long branchId)
        {
            await EmitEvent("kot-linked-to-sales", linkData, companyId, branchId);
        }

        /// <summary>
        /// Emit item status updated event
        /// </summary>
        public static async Task EmitItemStatusUpdated(object itemData, long companyId, long branchId, long? stationId = null)
        {
            await EmitEvent("item-status-updated", itemData, companyId, branchId, stationId);
        }

        /// <summary>
        /// Emit table status updated event
        /// </summary>
        public static async Task EmitTableStatusUpdated(object tableData, long companyId, long branchId)
        {
            await EmitEvent("table-status-updated", tableData, companyId, branchId);
        }

        /// <summary>
        /// Emit booking status updated event
        /// </summary>
        public static async Task EmitBookingStatusUpdated(object bookingData, long companyId, long branchId)
        {
            await EmitEvent("booking-status-updated", bookingData, companyId, branchId);
        }

        /// <summary>
        /// Emit booking linked to KOT event
        /// </summary>
        public static async Task EmitBookingLinkedToKot(object linkData, long companyId, long branchId)
        {
            await EmitEvent("booking-linked-to-kot", linkData, companyId, branchId);
        }
    }
}


