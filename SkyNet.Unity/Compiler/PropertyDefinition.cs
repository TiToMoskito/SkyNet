using System.Xml.Serialization;

namespace SkyNet.Compiler
{
    public class PropertyDefinition
    {        
        [XmlIgnore]
        public bool Deleted;
        [XmlElement]
        public int Adjust;
        [XmlElement]
        public string Name;
        [XmlElement]
        public string Type;
        [XmlElement]
        public AxisSelection PosAxe = AxisSelection.Disabled;
        [XmlElement]
        public AxisSelection RotAxe = AxisSelection.Disabled;
        [XmlElement]
        public AxisSelection VelAxe = AxisSelection.Disabled;
        [XmlElement]
        public AxisSelection AVelAxe = AxisSelection.Disabled;
        [XmlElement]
        public bool SmoothAlgorithm = false;
        [XmlElement]
        public bool CompressPos = false;
        [XmlElement]
        public float InterpolationBackTime = 0.1f;
        [XmlElement]
        public float ExtrapolationLimit = 0.3f;
        [XmlElement]
        public float ExtrapolationDistanceLimit = .3f;
        [XmlElement]
        public float PositionSnapThreshold = 8;
        [XmlElement]
        public float RotationSnapThreshold = 60;
        [XmlElement]
        public float PositionLerpSpeed = .2f;
        [XmlElement]
        public float RotationLerpSpeed = .2f;
        [XmlElement]
        public ObjDefinition objDefinition;
        [XmlElement]
        public ArrayDefinition ArrayDefinition;
        [XmlElement]
        public CompressorIntConfig IntCompression = new CompressorIntConfig
        {
            minValue = -1024,
            maxValue = 1024
        };
        [XmlElement]
        public CompressorFloatConfig FloatCompression = new CompressorFloatConfig
        {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
        };
        [XmlArray]
        public CompressorFloatConfig[] PositionCompression = new CompressorFloatConfig[3]
        {
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          }
        };
        [XmlArray]
        public CompressorFloatConfig[] RotationCompression = new CompressorFloatConfig[3]
        {
          new CompressorFloatConfig()
          {
            minValue = 0,
            maxValue = 360,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = 0,
            maxValue = 360,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = 0,
            maxValue = 360,
            precision = 0.01f
          }
        };
        [XmlArray]
        public CompressorFloatConfig[] VelocityCompression = new CompressorFloatConfig[3]
        {
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          }
        };
        [XmlArray]
        public CompressorFloatConfig[] AngularVelocityCompression = new CompressorFloatConfig[3]
        {
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          },
          new CompressorFloatConfig()
          {
            minValue = -1024,
            maxValue = 1024,
            precision = 0.01f
          }
        };
    }

    public enum AxisSelection
    {
        Disabled,
        X,
        Y,
        XY,
        Z,
        XZ,
        YZ,
        XYZ,
    }
}
