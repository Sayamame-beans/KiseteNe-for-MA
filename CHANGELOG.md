# Changelog

## [1.2.0] - 2023-12-31
### Added
- 調整値の保存/読み込みに対応 [`#5`](https://github.com/Sayamame-beans/KiseteNe-for-MA/issues/5)
  - KiseteNe for MAで行った調整を保存、共有することが出来ます
  - `ファイルに調整値を保存する`
    - jsonファイルに調整値を出力します
  - `ファイルから調整値を(現在の状態として)読み込む`
    - 現在のアバターの状態をファイルの内容が適用されている状態として、KiseteNe for MAの表示と内部状態を揃えます
    - これにより、調整を途中から再開することが出来ます
    - デフォルト値の再現に誤差が生まれることがあり、RESETを押しても調整前と全く同じ状態にはならない場合があります
  - `ファイルから調整値を読み込み、新たに適用する`
    - 現在のアバターの状態をデフォルトの状態として、ファイルの内容をアバターに適用します

### Fixed
- 袖/裾について、伸ばす操作と太くする操作が入れ替わることがある問題を修正 [`#6`](https://github.com/Sayamame-beans/KiseteNe-for-MA/issues/6)
  - 逆に、この修正によって操作が入れ替わるようになる衣装もあるかもしれません
  - 問題が発生した場合はご報告ください
- デフォルトのScale値が1以外である場合が想定されていなかった問題を修正 [`#15`](https://github.com/Sayamame-beans/KiseteNe-for-MA/issues/15)

## [1.1.0] - 2023-09-11
### Added
- Undo/Redoに対応 [`#7`](https://github.com/Sayamame-beans/KiseteNe-for-MA/pull/7)
  - KiseteNe for MAウィンドウ上の全操作/値と衣装側のTransform変化をUndo/Redoすることが出来ます
  - ウィンドウを閉じてからUndo/Redoを行うと、ウィンドウ上の操作/値は復元されず、衣装側のTransformのみ復元されます
  - 上記の理由から、ウィンドウを一度閉じてからUndo/Redoを行った場合は何も起こらないことがありますが、仕様です
- Changelogを追加 [`#9`](https://github.com/Sayamame-beans/KiseteNe-for-MA/issues/9)

### Changed
- アクセス修飾子を修正 [`#8`](https://github.com/Sayamame-beans/KiseteNe-for-MA/issues/8)

## [1.0.1] - 2023-08-31
### Fixed
- VPAI Packageの生成設定に誤りがあったため修正
- `vpmDependencies`のtypoを修正

## [1.0.0] - 2023-08-31
### Added
- VPMに対応
- prefabでも(unpackせずとも)利用出来るように

### Removed
- 着せる機能を削除(Modular Avatarと併用する想定であるため)
