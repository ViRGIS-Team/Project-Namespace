// copyright Runette Software Ltd, 2020-26. All rights reserved
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using Virgis;
using System.Linq;

namespace Project
{
    public class GisProject : GisProjectPrototype
    {
        [JsonIgnore]
        public override string path
        {
            set
            {
                foreach (RecordSet set in RecordSets)
                {
                    set.path = value;
                }
            }
        }
        protected override string TYPE  { get => "virgis";}
        protected override string VERSION { get => "3.0.0";}

        [JsonProperty(PropertyName = "recordsets", Required = Required.Always)]
        [JsonConverter(typeof(RecordsetConverter))]
        public new List<RecordSet> RecordSets;
    }

    public class RecordSet : RecordSetPrototype
    {
        [JsonIgnore]
        private string m_Path;

        [JsonIgnore]
        public string path { get { return m_Path; } set
            {
                m_Path= value;
                Properties.path = value;
                if ( Units != null ) foreach(Unit unit in Units.Values)
                {
                    unit.path = value;
                }
            } }

        [JsonProperty(PropertyName = "datatype", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public RecordSetDataType DataType;
        public override string Source 
        { get { return Path.GetFullPath(Path.Combine( path, m_source)); }
          set { m_source = value; } 
        }
        [JsonProperty(PropertyName = "properties")]
        public GeogData Properties;
        [JsonProperty(PropertyName = "proj4")]
        public string Crs;

        /// <summary>
        /// Dictionary of symbology units for this layer
        /// </summary>
        [JsonProperty(PropertyName = "units", NullValueHandling = NullValueHandling.Ignore)]
        public new Dictionary<string, Unit> Units = new();

        /// <summary>
        /// List of Data Units for this layer
        /// </summary>
        [JsonProperty(PropertyName = "data_units")]
        public new List<DataUnit> DataUnits;
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
                    foreach (JObject set in sets)
                    {
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

    /// <summary>
    /// Object that holds the layer properties
    /// </summary>
    public class GeogData : PropertiesPrototype
    {

        [JsonIgnore]
        private string m_Path;

        [JsonIgnore]
        public string path { 
            get { return m_Path; }
            set { 
                m_Path = value;
                bhdata.path = value;
            }
        }

        [JsonProperty(PropertyName = "hide-sublayers")]
        public List<string> hideSublayers;
    }


    /// <summary>
    /// Acceptable values for Recordset Type
    /// </summary>
    public enum RecordSetDataType{
        Vector,
        Raster,
        PointCloud,
        Mesh,
        Mdal,
        Point,
        Line,
        Polygon,
        CSV
    }



    public class Unit : UnitPrototype
    {
        [JsonIgnore]
        public string path;

        /// <summary>
        /// The transfor to be applied to the unit of symnbology
        /// </summary>

        [JsonProperty(PropertyName = "texture-image")]
        public string m_TextureImage;

        [JsonIgnore]
        public string TextureImage
        {
            get
            {
                if (m_TextureImage is not null && m_TextureImage != "")
                {
                    return Path.GetFullPath(Path.Combine(path, m_TextureImage));
                }
                else
                {
                    return null;
                }
            }
        }

    }

    public class DataUnit: DataUnitPrototype {
        /// <summary>
        /// Dictionary of symbology units for this data unit
        /// </summary>
        [JsonProperty(PropertyName = "units")]
        public new Dictionary<string, Unit> Units;
    }
}
