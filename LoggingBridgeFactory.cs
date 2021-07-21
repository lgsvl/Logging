/**
 * Copyright (c) 2020-2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System;
using Newtonsoft.Json;
using Simulator.Bridge.Data;
using UnityEngine;

namespace Simulator.Bridge
{
    // name should match bridge plugin name (main C# file, and folder name under External/Bridges folder)
    // this will also be used to display name in UI
    [BridgeName("Logging", "Logging")]
    public class LoggingBridgeFactory : IBridgeFactory
    {
        // called to create unique instance associated with vehicle
        public IBridgeInstance CreateInstance() => new LoggingBridgeInstance();

        public LoggingBridgeFactory()
        {
            jsonSettings.Formatting = Formatting.None;
            jsonSettings.Converters.Add(new UnityConverters());
        }

        public void Register(IBridgePlugin plugin)
        {
            // here we need to register which types to we support for this bridge, we support all
            // simulator default types from Simulator.Bridge.Data namespace that can be published

            // we will convert all types to json as C# string that will be written to file
            // which means we do not need special converison function
            RegPublisher<CanBusData, string>(plugin, null);
            RegPublisher<ClockData, string>(plugin, null);
            RegPublisher<Detected2DObjectData, string>(plugin, null);
            RegPublisher<Detected3DObjectData, string>(plugin, null);
            RegPublisher<DetectedRadarObjectData, string>(plugin, null);
            RegPublisher<GpsData, string>(plugin, null);
            RegPublisher<GpsOdometryData, string>(plugin, null);
            RegPublisher<ImageData, string>(plugin, null);
            RegPublisher<ImuData, string>(plugin, null);
            RegPublisher<CorrectedImuData, string>(plugin, null);
            RegPublisher<PointCloudData, string>(plugin, null);
            RegPublisher<SignalDataArray, string>(plugin, null);
            RegPublisher<VehicleOdometryData, string>(plugin, null);

            // what subscribers we support
            plugin.AddType<VehicleControlData>(typeof(VehicleControlData).Name);
            plugin.AddSubscriberCreator<VehicleControlData>((instance, topic, callback) => { });
        }


        // this function can be called not only by our "Register" method, but also
        // from custom sensor plugins
        public void RegPublisher<DataType, BridgeType>(IBridgePlugin plugin, Func<DataType, BridgeType> converter)
        {
            // register "native name" for data type, we just use name of original tpye
            plugin.AddType<DataType>(typeof(DataType).Name);

            // then we provide delegate to create publishers on specific topic
            // writing functionality is delegated to actual bridge instance
            plugin.AddPublisherCreator(
                (instance, topic) =>
                {
                    var loggingInstance = instance as LoggingBridgeInstance;

                    // as we have very simple bridge that does not require "conversion", we'll just call
                    // serializer directly on simulator data types. For real bridges you would need to convert data
                    // types (DataType) to something that your native bridge implementation supports (BridgeType)
                    ;
                    return new Publisher<DataType>(
                        (data, completed) => loggingInstance.Write(typeof(DataType).Name, topic, JsonConvert.SerializeObject(data, jsonSettings), completed));
                }
            );
        }

        // we do not support subscribers
        public void RegSubscriber<DataType, BridgeType>(IBridgePlugin plugin, Func<BridgeType, DataType> converter)
        {
            throw new NotSupportedException("LoggingBridge does not support subscribers");
        }

        static JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
    }

    // some unity types require special json conversion logic
    class UnityConverters : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion)
                || objectType == typeof(Vector3)
                || objectType == typeof(Matrix4x4);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(Quaternion))
            {
                var q = (Quaternion)value;
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(q.x);
                writer.WritePropertyName("y");
                writer.WriteValue(q.y);
                writer.WritePropertyName("z");
                writer.WriteValue(q.z);
                writer.WritePropertyName("w");
                writer.WriteValue(q.w);
                writer.WriteEndObject();
            }
            else if (value.GetType() == typeof(Vector3))
            {
                var v = (Vector3)value;
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(v.x);
                writer.WritePropertyName("y");
                writer.WriteValue(v.y);
                writer.WritePropertyName("z");
                writer.WriteValue(v.z);
                writer.WriteEndObject();
            }
            else if (value.GetType() == typeof(Matrix4x4))
            {
                var m = (Matrix4x4)value;
                writer.WriteStartArray();
                for (int i = 0; i < 16; i++)
                {
                    writer.WriteValue(m[i]);
                }
                writer.WriteEnd();
            }
        }
    }
}
