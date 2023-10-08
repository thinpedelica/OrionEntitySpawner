using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrionEntitySpawner {
    [System.Serializable]
    public class EntityInfo {
        public string id;
        public string type;
        public Direction direction;
        public Location location;
    }

    [System.Serializable]
    public class Direction {
        public string type;
        public float value;
        public Metadata metadata;
    }

    [System.Serializable]
    public class Location {
        public string type;
        public GeoValue value;
        public Metadata metadata;
    }

    [System.Serializable]
    public class GeoValue {
        public string type;
        public float[] coordinates;
    }

    [System.Serializable]
    public class Metadata {
        // future use.
    }
}
