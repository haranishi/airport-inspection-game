# 検査ゲームの判定・勤務状態の設計

2026-10-30。READMEの1勤務6ケースの範囲を引き継ぎ、現在の判定規則とアプリケーション境界を記録する。

## 成功条件と対象
- プレイヤーが固定6ケースを調べ、通過・没収・通報を選び、物と規則の根拠を確認して勤務結果へ進める。
- 物・規則は固定の架空データ。実際の保安判断や回避手順を扱う設計ではない。
- 保存、2日目、分岐、Steam連携は未実装。今回の文書追加でその範囲を変更しない。

## 依存境界
| 層 | 正本と責務 |
| --- | --- |
| Domain | [InspectionModels.cs](../Assets/AirportInspection/Domain/InspectionModels.cs)で物/規則/ケース/判定結果、[InspectionEvaluator.cs](../Assets/AirportInspection/Domain/InspectionEvaluator.cs)で判定 |
| Application | [ShiftSession.cs](../Assets/AirportInspection/Application/ShiftSession.cs)でケース進行、[VerticalSliceContent.cs](../Assets/AirportInspection/Application/VerticalSliceContent.cs)で固定コンテンツ |
| Presentation | [AirportGameController.cs](../Assets/AirportInspection/Presentation/AirportGameController.cs)で入力・X線表現・根拠表示、[AirportGameBootstrap.cs](../Assets/AirportInspection/Presentation/AirportGameBootstrap.cs)で起動 |
| Editor | [AirportInspectionBuild.cs](../Assets/Editor/AirportInspectionBuild.cs)で空SceneとmacOS開発ビルドの生成 |

判定と勤務進行のコードはUnityEngine型を使わず、表示/入力をPresentationへ置いている。

## 判定契約
- `BagCase.Items`の`CategoryId`と各`RuleDefinition.CategoryId`を照合する。
- 一致した規則のうち`Report > Confiscate > Pass`の優先順位で期待する措置を決める。
- 同じ強さの規則では、走査中に先に見つかった物/規則の根拠を維持する。
- 一致が無ければ`Pass`と`R-CLEAR`、根拠物IDは空文字。選択と期待する措置の一致が`IsCorrect`になる。
- 結果は選択・期待措置、規則ID/文、根拠物ID/名を返し、画面はこれを根拠の枠表示と説明に使う。

## 勤務の状態
| 状態/操作 | 変化とガード |
| --- | --- |
| 作成直後 | `CaseIndex=0`、正答数0、判定結果待ちなし |
| `Decide` | 未完了かつ結果待ちなしでだけ判定。正答なら加算し、`HasPendingResult=true` |
| 結果確認 | 再判定は例外。画面でもモーダル/完了/結果待ち中の検査入力を拒否する |
| `Advance` | 判定済みの場合だけ次へ。最終ケース後は`IsComplete=true` |
| `Restart` | 同じ固定ケースと規則で新しい`ShiftSession`を作る |

- 回転・材質表示・開披は調べ方の表示であり、固定の物や判定規則そのものを書き換えない。
- 結果を確認する前に進む操作、同一ケースへの二重判定は`ShiftSession`が拒否する。

## データと生成物
- ケース、規則、現在位置、正答数はメモリ内。プレイヤーの実データ、アカウント、外部通信、永続保存は使わない。
- Editorの`PrepareScene`は`Assets/Scenes/Inspection.unity`を空Sceneとして作り、BuildSettingsへ登録する。
- `BuildMac`はSceneを準備した後に開発用macOSアプリを作る。これらは生成物を変更するため、文書確認では実行しない。

## 検証条件と今回の限界
- [既存EditModeテスト](../Assets/AirportInspection/Tests/EditMode/InspectionEvaluatorTests.cs)は正常・没収・通報、規則の優先、進行ガード、6ケース完走を確認する6件。
- 規則やケースを変えるときは、同順位の根拠順序、根拠ID、正答数、完了後の再開始も確認する。
- 実操作はREADMEの既存手順でScene生成・EditMode・ビルドを直列に行い、入力と根拠表示を確認する。
- 既存のUnity/ライセンスIPC制約を維持し、ロックやライセンスIPCを削除して回避しない。
- 今回はコード変更0の文書確認。Unity起動・テスト・ビルド・実操作は行わず、既存の未検証事項を成功へ変更していない。
