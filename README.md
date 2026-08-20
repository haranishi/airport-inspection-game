# Airport Inspection Game — local vertical slice

架空空港の固定ブースで、X線風表示から荷物を検査するUnityローカル試作です。現在はコード作成済み・Unity未検証であり、完成ビルドではありません。

## 今ある範囲

- 1勤務6ケース：正常4、没収1、警備通報1
- 純粋C#の規則判定と根拠ID
- 固定ブース、仮図形X線、90度回転、材質フィルター、開披
- 通過／没収／通報と、物・規則を示す判定後フィードバック
- EditModeテスト6件、空Scene生成、macOS開発ビルド用スクリプト

外部通信、購入素材、実在の危険物名や保安回避手順は使っていません。保存、2日目、物語分岐、Steam連携は未実装です。

## 必要環境

- Unity `6000.3.20f1`
- macOS Editor。Windows版は未導入のため、現環境ではmacOSローカルビルドを検証対象にする

## 検証・起動

別のUnity batchmodeと同時実行しないでください。最初にScene生成兼コンパイルを行います。

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -quit -projectPath "/Users/hara/Projects/airport-inspection-game" -executeMethod AirportInspection.Editor.AirportInspectionBuild.PrepareScene -logFile "/private/tmp/airport-compile.log"
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -quit -projectPath "/Users/hara/Projects/airport-inspection-game" -runTests -testPlatform EditMode -testResults "/private/tmp/airport-editmode.xml" -logFile "/private/tmp/airport-tests.log"
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics -quit -projectPath "/Users/hara/Projects/airport-inspection-game" -executeMethod AirportInspection.Editor.AirportInspectionBuild.BuildMac -logFile "/private/tmp/airport-build.log"
open "/Users/hara/Projects/airport-inspection-game/Builds/Mac/Airport Inspection.app"
```

Editorでは`Assets/Scenes/Inspection.unity`を開いてPlayします。`R`回転、`F`材質、`O`開披、`1/2/3`判定です。

## 現在の注意

初回生成時にUnity Package ManagerのIPC制限とLicensing Client再接続停止が起きました。手動でロックやライセンスIPCを削除せず、他のUnity処理終了後に上記を直列実行してください。詳細はObsidianの`07_実装進捗・再開メモ`を正本とします。
