using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using PLATEAU.CityInfo;
using PLATEAU.Native;

using GeoCoordinateUtility;

namespace OrionEntitySpawner
{
    public class EntityPositionUpdater : MonoBehaviour
    {
        private string _baseUrl;
        private string _id;

        private float _intervalSec;
        private float _elapsedTime = 0f;
        private PlateauVector3d _originalPoint;
        private GeoPoint _originalCoordinate;

        void Update()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > _intervalSec) {
                StartCoroutine(UpdatePosition());
                _elapsedTime = 0f;
            }        
        }

        public void Initialize(string baseUrl, string id, PlateauVector3d originalPoint, int zoneId, float intervalSec) {
            _baseUrl = baseUrl;
            _id = id;
            _originalPoint = originalPoint;
            _originalCoordinate = GeoCoordinateConverter.ZoneId2LatLon(zoneId);
            _intervalSec = intervalSec;
            StartCoroutine(UpdatePosition());
        }

        IEnumerator UpdatePosition() {
            string getUrl = _baseUrl + "/v2/entities/" + _id;
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

            EntityInfo entityInfo = JsonUtility.FromJson<EntityInfo>(jsonText);
            var coordinates = entityInfo.location.value.coordinates;
            var geoPoint = GeoCoordinateConverter.LatLon2Coordinate(coordinates[1], coordinates[0], _originalCoordinate.X, _originalCoordinate.Y);

            var pvecPoint = RegularCoordinate2Plateau(geoPoint.X, 0, geoPoint.Y);
            transform.position = new Vector3((float)pvecPoint.X, (float)pvecPoint.Y, (float)pvecPoint.Z);
        }

        private PlateauVector3d RegularCoordinate2Plateau(double x, double y, double z) {
            // 平面直角座標系は、X軸が南北（真北が正）、Y軸が東西（真東が正）
            // Plateauは、X軸が東西、Y軸が南北
            return new PlateauVector3d(z - _originalPoint.X, y - _originalPoint.Y, x - _originalPoint.Z);
        }
    }
}
