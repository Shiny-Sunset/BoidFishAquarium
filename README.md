# BoidFishAquarium

Boids Algorithm を用いた魚群シミュレーションアプリです。水槽の中を泳ぐ魚の群れをリアルタイムで観察できます。

**Play:** [https://shiny-sunset.github.io/BoidFishAquarium/](https://shiny-sunset.github.io/BoidFishAquarium/)

## 遊び方

画面左上に現在の操作キーが常に表示されます。

### 準備フェーズ (Setup)

水槽内の環境を自由にカスタマイズしてからシミュレーションを開始できます。

- 魚の数をスライダーで調整
- 水槽内に障害物を配置
- **A / D** — カメラ回転
- **Start** ボタンでシミュレーション開始

### シミュレーションフェーズ

魚の群れが Boids Algorithm に基づいて泳ぎ始めます。

- **F** — 餌を与える
- **G** — 魚を驚かす
- **A / D** — カメラ回転
- **Tab** — 潜水モードに切り替え
- **Backspace** — リセット（準備フェーズに戻る）

### 潜水モード

水槽の中に潜って、魚を間近で観察できます。

- **WASD** — 移動
- **マウス** — 視点操作
- **F** — 餌を射出
- **G** — 障害物を射出
- **Tab** — 通常モードに戻る

## 技術

- **Boids Algorithm** — 分離 (Separation)・整列 (Alignment)・結合 (Cohesion) の3つのルールで群れの動きを再現
