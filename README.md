# Orion Entity Spawner
Orion Entity Spawnerは、オープンソースの都市OS:FIWAREの[Orion](https://fiware-orion.letsfiware.jp/)と接続し、  
Orionに登録されたエンティティ(モビリティ、店舗、ロボットなど)をPLATEAUの3D都市モデル上に配置する機能を提供します。

# 利用手順
1. Hierarchyで「Create Empty」によりGameObject(EntitySpawnerObjectとする)を作成する。  
2. EntitySpawnerObjectのInspectorウィンドウで「Add Component」を押下し、EntitySpawnerを選択する。
3. Entityとして表示したい3DモデルのPrefab(EntityPrefabとする)を作成する。
4. EntityPrefabのInspectorウィンドウで「Add Component」を押下し、EntityPositionUpdaterを選択する。
5. EntitySpawnerObjectのInspectorウィンドウで、以下のパラメタを設定する。  
  - BaseURL: OrionのURL
  - Entity Prefab: 3. で製作したPrefab
  - City Model: PLATEAU SDKでimportした3D都市のトップのGameObject(例：13100_tokyo23-ku_2022_citygml_1_1_op)
  - Interval Sec: Orionからデータを取得する頻度(デフォルト：3秒)
