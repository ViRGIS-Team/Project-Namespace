

using System.Collections.Generic;
using System.Linq;
using System;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace Project
{
    public class GisProject : TestableObject
    {
        private const string TYPE = "project";
        private const string VERSION = "1.0.1";

        public static string GetVersion()
        {
            return $"{TYPE}:{VERSION}";
        }
        
        [JsonProperty(PropertyName = "version", Required = Required.Always)]
        public string ProjectVersion;

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name;

        [JsonProperty(PropertyName = "origin", Required = Required.Always)]
        public Point Origin;

        [JsonProperty(PropertyName = "scale", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(1f)]
        public float Scale;

        [JsonProperty(PropertyName = "default_proj")]
        public string projectCrs;

        [JsonProperty(PropertyName = "grid-scale")]
        public float GridScale;

        [JsonProperty(PropertyName = "cameras", Required = Required.Always)]
        public List<Point> Cameras;

        [JsonProperty(PropertyName = "recordsets", Required = Required.Always)]
        [JsonConverter(typeof(RecordsetConverter))]
        public List<RecordSet> RecordSets;
    }

    public class RecordSet : TestableObject
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id;
        [JsonProperty(PropertyName = "display-name")]
        public string DisplayName;
        [JsonProperty(PropertyName = "datatype", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public RecordSetDataType DataType;
        [JsonProperty(PropertyName = "source")]
        public string Source;  
        [JsonProperty(PropertyName = "position")]
        public Point Position;
        [JsonProperty(PropertyName = "transform")]
        public JsonTransform Transform;
        [JsonProperty(PropertyName = "properties")]
        public GeogData Properties;
        [JsonProperty(PropertyName = "visible", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(true)]
        public bool Visible;
        [JsonProperty(PropertyName = "proj4")]
        public string Crs;
    }

    public class JsonTransform : TestableObject
    {
        [JsonProperty(PropertyName = "translate", Required = Required.Always)]
        [JsonConverter(typeof(VectorConverter<SerializableVector3>))]
        public SerializableVector3 Position;
        [JsonProperty(PropertyName = "rotate", Required = Required.Always)]
        [JsonConverter(typeof(VectorConverter<SerializableQuaternion>))]
        public SerializableQuaternion Rotate;
        [JsonProperty(PropertyName = "scale", Required = Required.Always)]
        [JsonConverter(typeof(VectorConverter<SerializableVector3>))]
        public SerializableVector3 Scale;
    }

    public class VectorConverter<T>  : JsonConverter where T: Serializable, new()
    {
        public VectorConverter()
        {

        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartArray:
                    JArray jarray = JArray.Load(reader);
                    IList<float> values = jarray.Select(c => (float)c).ToList();
                    T result = new T();
                    result.Update(values);
                    return result;
            }

            throw new JsonReaderException("expected null, object or array token but received " + reader.TokenType);
        }


        public override void WriteJson(JsonWriter writer, object vector, JsonSerializer serializer)
        {
            T newvector = (T)vector;
            serializer.Serialize(writer, newvector.ToArray());
        }
    }

    public class RecordsetConverter : JsonConverter
    {
        public RecordsetConverter()
        {

        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(RecordSet).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return null;
                case JsonToken.StartArray:
                    JArray jarray = JArray.Load(reader);
                    IList<JObject> sets = jarray.Select(c => (JObject)c).ToList();
                    List<RecordSet> result = new List<RecordSet>();
                    foreach (JObject set in sets) {
                        result.Add(set.ToObject(typeof(RecordSet)) as RecordSet);
                    }
                    return result;
            }

            throw new JsonReaderException("expected null, object or array token but received " + reader.TokenType);
        }


        public override void WriteJson(JsonWriter writer, object vector, JsonSerializer serializer)
        {
            serializer.Serialize(writer, vector);
        }
    }

    public struct GeogData {
        [JsonProperty(PropertyName = "units")]
        public Dictionary<string, Unit> Units;
        [JsonProperty(PropertyName = "dem")]
        public string Dem;
        [JsonProperty(PropertyName = "colorinterp")]
        public Dictionary<string, object> ColorInterp;
        [JsonProperty(PropertyName = "filter")]
        public List<Dictionary<string, object>> Filter;
        [JsonProperty(PropertyName = "bbox")]
        public List<double> BBox;
        [JsonProperty(PropertyName = "source-type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(SourceType.File)]
        public SourceType SourceType;
        [JsonProperty(PropertyName = "read-only", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool ReadOnly;
        [JsonProperty(PropertyName = "mapscale")]
        public Int32 MapScale;
        [JsonProperty(PropertyName = "map_size")]
        public int MapSize;
        [JsonProperty(PropertyName = "elevation_source_type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("MapboxTerrain")]
        public string elevationSourceType;
        [JsonProperty(PropertyName = "elevation_layer_type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("FlatTerrain")]
        public string elevationLayerType;
        [JsonProperty(PropertyName = "imagery_source_type", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("MapboxOutdoors")]
        public string imagerySourceType;
    }


    /// <summary>
    /// Acceptable values for Recordset Type
    /// </summary>
    public enum RecordSetDataType{
        MapBox,
        Vector,
        Raster,
        PointCloud,
        Mesh,
        Mdal,
        Point,
        Line,
        Polygon,
        DEM,
        Graph
    }


    /// <summary>
    /// Acceptable values for the Source field of a recordset
    /// </summary>
    public enum SourceType {
        File,
        WFS,
        OAPIF,
        WMS,
        WCS,
        DB,
        AWS,
        GCS,
        Azure,
        Alibaba,
        Openstack,
        TCP,
    }


    public class Unit : TestableObject
    {
        /// <summary>
        /// Color used for the unit of symbology.
        /// 
        /// Can be in either integer[0 .. 255] format or float[0..1] format
        /// </summary>
        [JsonProperty(PropertyName = "color", Required = Required.Always)]
        [JsonConverter(typeof(VectorConverter<SerializableColor>))]
        public SerializableColor Color;
        /// <summary>
        /// The shape to be used by the unit of symbology.
        /// 
        /// Must contain an instance of Shapes
        /// </summary>
        [JsonProperty(PropertyName = "shape", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Shapes Shape;
        /// <summary>
        /// The transfor to be applied to the unit of symnbology
        /// </summary>
        [JsonProperty(PropertyName = "transform", Required = Required.Always)]
        public JsonTransform Transform;
        /// <summary>
        /// The name of a field in the metadata to be used a label for the data entity
        /// </summary>
        [JsonProperty(PropertyName = "label")]
        public string Label;
    }

    /// <summary>
    /// Acceptable value for the Shape field in Symbology
    /// </summary>
    public enum Shapes
    {
        Spheroid,
        Cuboid,
        Cylinder
    }

    /// <summary>
    /// Generic class to make an entity testabble - to allow the members to be tested for their presence
    /// </summary>
    public class TestableObject
    {
        public bool ContainsKey(string propName)
        {
            return GetType().GetMember(propName) != null;
        }
    }

}