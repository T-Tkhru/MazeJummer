# 更新履歴
- 2025/7/6：ReadMe作成

# ゲーム概要
2人対戦迷路脱出ゲームです。脱出側（Runner）と妨害側（Trapper）に分かれて対戦します
- Runner側は迷路に散らばる２つのカギを取得してゴールに向かいます
- Trapper側は迷路にトラップや壁を生成することでRunnerの脱出を妨害します
- N分以内に脱出できればRunnerの勝ち、脱出できなければTrapperの勝ちです

# 環境
Unity6（6000.1.3f1）

# 起動方法
このリポジトリをcloneし、UnityHubで上記の環境を指定して開いてください。

エディタが開いたら、上側のメニューバーから「Window」→「Multiplayer」→「Multiplayer Play Mode」を開き、Virtual PlayersのPlayer2にチェックを入れると、ビルドせずに2画面でプレイができます。
またPCを2台用意してそれぞれの環境でエディタを起動しても、マルチプレイを再現できます。

# ビルド
ビルドはWindows環境（.exe）を想定しています。メニューバーの「File」→「Build Profiles」を開き、Windowsが選択されているのを確認してビルドしてください。
他の環境でのビルド検証結果は以下の通りです。
- WebGL：UnityRoomにて検証。Windows以外のOSでは動作確認できた。WindowsのみUIは表示されるがCameraがレンダリングされず灰色一色になってしまう。
