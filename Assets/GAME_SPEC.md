# MazeJummer 仕様書

## ゲーム概要

- ランナー（脱出者）とトラッパー（妨害者）に分かれて対戦するマルチプレイ迷路脱出ゲーム。
- 3 分以内にランナーがゴールすればランナー勝利、脱出できなければトラッパー勝利。

## ルール

- ランナーは迷路内を移動し、ゴールを目指す。
- トラッパーは迷路内に壁やトラップを設置し、ランナーの脱出を妨害する。
- ランナーの周囲 2 マスにはトラップ設置不可。

## 操作方法

### ランナー

- 移動：WASD
- 視点移動：マウス（左右方向のみ）
- Esc/Alt キーでマウスカーソル表示・非表示切替

### トラッパー

- トラップ/壁の生成：トラップ項目選択 → 設置位置クリック

## 主要システム・UI

- `GameManager`：ゲーム進行管理（開始/終了判定、制限時間、勝敗判定）
- `MazeManager`：迷路生成・管理
- `RunnerUIManager`：ランナー用 UI（タイマー、トラップ効果表示、カウントダウン、結果 UI）
- `TrapperUIManager`：トラッパー用 UI（迷路 UI、トラップ設置、プレイヤー位置表示、サブカメラ、結果 UI）

## トラップ一覧

- 壁：通行不可ブロック
- スピードダウントラップ：ランナーの移動速度低下
- ブラインドトラップ：視界を遮る
- リバース入力トラップ：操作反転

## ゴール・キー

- ゴール位置 UI、キー取得 UI あり
- キー取得でゴール解放などのギミック

## プレハブ・リソース

- UI プレハブ（RunnerUI, TrapperUI, TrapUI, KeyUI, ResultUI 等）
- サブカメラ用 Prefab・RenderTexture
- フォント：TextMesh Pro, LightNovelPOPv2.otf, LiberationSans.ttf
- アニメーション：UnityChanRunController.controller
- マテリアル：CircleMask.shader

## キャラクター

- SD ユニティちゃん（UnityChan）モデル利用
- Mecanim/Humanoid 形式 Animator
- Toon Shader 対応

## プラグイン

- DOTween（Demigiant）：アニメーション制御
- TextMesh Pro：UI テキスト

## シーン・その他

- サンプルシーン（UnityChan 関連）あり
- 各種 README（UnityChan ライセンス、利用方法等）
