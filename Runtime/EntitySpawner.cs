using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using PLATEAU.CityInfo;
using PLATEAU.Native;

using static OrionEntitySpawner.EntityPositionUpdater;

namespace OrionEntitySpawner
{
    public class EntitySpawner : MonoBehaviour
    {
        public string baseUrl = "http://localhost:1026";
        public GameObject entityPrefab;

        public PLATEAUInstancedCityModel cityModel;

        public float intervalSec = 3f;
        private float _elapsedTime = 0f;

        private PlateauVector3d _originalPoint;
        private int _zoneId;

        private List<string> _idList = new List<string>();



        void Start()
        {
            _originalPoint = cityModel.GeoReference.ReferencePoint;
            _zoneId = cityModel.GeoReference.ZoneID;
            // Debug.Log(_originalPoint);
        }

        void Update()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > intervalSec) {
                StartCoroutine(Spawn());
                _elapsedTime = 0f;
            }        
        }

        IEnumerator Spawn() {
            string getUrl = baseUrl + "/v2/entities";
            UnityWebRequest request = UnityWebRequest.Get(getUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError) {
                Debug.Log(request.error);
                yield break;
            }
            if (request.responseCode != 200) {
                Debug.Log(request.responseCode);
                yield break;
            }

            string jsonText = request.downloadHandler.text;
            // Debug.Log(jsonText);

            var entityInfoJsonTextList = ParseEntityInfoStringList(jsonText);
            foreach (var entityInfoJsonText in entityInfoJsonTextList) {
                // Debug.Log(entityInfoJsonText);
                EntityInfo entityInfo = JsonUtility.FromJson<EntityInfo>(entityInfoJsonText);
                var result = _idList.Exists(x => x.Equals(entityInfo.id));
                if (!result) {
                    _idList.Add(entityInfo.id);
                    GameObject entity = Instantiate(entityPrefab);
                    EntityPositionUpdater entityPositionUpdater = entity.GetComponent<EntityPositionUpdater>();
                    entityPositionUpdater.Initialize(baseUrl, entityInfo.id, _originalPoint, _zoneId, intervalSec);
                }
                yield return null;
            }
        }

        private List<string> ParseEntityInfoStringList(string text) {
            List<string> resultList = new List<string>();
            if (text[0] != '[' && text[-1] != ']') {
                return resultList;
            }
            text = text.TrimStart('[');
            text = text.TrimEnd(']');

            var separateText = "\"metadata\":{}}}";
            var splited = text.Split(separateText);
            for (int index = 0; index < splited.Length - 1; index++) {
                var workText = splited[index];
                if (workText[0] == ',') {
                    workText = workText.TrimStart(',');
                }
                resultList.Add(workText + separateText);
            }

            return resultList;
        }
    }
}
