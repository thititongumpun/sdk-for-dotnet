
using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Appwrite.Models
{
    public class Message
    {
        [JsonProperty("$id")]
        public string Id { get; private set; }

        [JsonProperty("$createdAt")]
        public string CreatedAt { get; private set; }

        [JsonProperty("$updatedAt")]
        public string UpdatedAt { get; private set; }

        [JsonProperty("providerType")]
        public string ProviderType { get; private set; }

        [JsonProperty("topics")]
        public List<object> Topics { get; private set; }

        [JsonProperty("users")]
        public List<object> Users { get; private set; }

        [JsonProperty("targets")]
        public List<object> Targets { get; private set; }

        [JsonProperty("scheduledAt")]
        public string? ScheduledAt { get; private set; }

        [JsonProperty("deliveredAt")]
        public string? DeliveredAt { get; private set; }

        [JsonProperty("deliveryErrors")]
        public List<object>? DeliveryErrors { get; private set; }

        [JsonProperty("deliveredTotal")]
        public long DeliveredTotal { get; private set; }

        [JsonProperty("data")]
        public object Data { get; private set; }

        [JsonProperty("status")]
        public string Status { get; private set; }

        [JsonProperty("description")]
        public string? Description { get; private set; }

        public Message(
            string id,
            string createdAt,
            string updatedAt,
            string providerType,
            List<object> topics,
            List<object> users,
            List<object> targets,
            string? scheduledAt,
            string? deliveredAt,
            List<object>? deliveryErrors,
            long deliveredTotal,
            object data,
            string status,
            string? description
        ) {
            Id = id;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            ProviderType = providerType;
            Topics = topics;
            Users = users;
            Targets = targets;
            ScheduledAt = scheduledAt;
            DeliveredAt = deliveredAt;
            DeliveryErrors = deliveryErrors;
            DeliveredTotal = deliveredTotal;
            Data = data;
            Status = status;
            Description = description;
        }

        public static Message From(Dictionary<string, object> map) => new Message(
            id: map["$id"].ToString(),
            createdAt: map["$createdAt"].ToString(),
            updatedAt: map["$updatedAt"].ToString(),
            providerType: map["providerType"].ToString(),
            topics: ((JArray)map["topics"]).ToObject<List<object>>(),
            users: ((JArray)map["users"]).ToObject<List<object>>(),
            targets: ((JArray)map["targets"]).ToObject<List<object>>(),
            scheduledAt: map["scheduledAt"]?.ToString(),
            deliveredAt: map["deliveredAt"]?.ToString(),
            deliveryErrors: ((JArray)map["deliveryErrors"]).ToObject<List<object>>(),
            deliveredTotal: Convert.ToInt64(map["deliveredTotal"]),
            data: map["data"].ToString(),
            status: map["status"].ToString(),
            description: map["description"]?.ToString()
        );

        public Dictionary<string, object?> ToMap() => new Dictionary<string, object?>()
        {
            { "$id", Id },
            { "$createdAt", CreatedAt },
            { "$updatedAt", UpdatedAt },
            { "providerType", ProviderType },
            { "topics", Topics },
            { "users", Users },
            { "targets", Targets },
            { "scheduledAt", ScheduledAt },
            { "deliveredAt", DeliveredAt },
            { "deliveryErrors", DeliveryErrors },
            { "deliveredTotal", DeliveredTotal },
            { "data", Data },
            { "status", Status },
            { "description", Description }
        };
    }
}