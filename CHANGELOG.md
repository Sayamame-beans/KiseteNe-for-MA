# Changelog

## [Unreleased]
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
