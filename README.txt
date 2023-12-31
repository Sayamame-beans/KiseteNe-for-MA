本ツールは、着せ替え支援ツール「キセテネ」をprefab状態の衣装にも利用可能なように改変したものです。
Modular Avatar等と併用することを前提としているため、衣装変形機能はそのままに、衣装を着せる機能は削除しています。
使い方は元のキセテネと殆ど同じで、Unityの画面上部メニューの「Tools」→「KiseteNe for MA」からウィンドウを開くことが出来ます。

Unity 2019.4とUnity 2022.3で動作確認済みです。

Modular Avatar
https://modular-avatar.nadena.dev/ja/

VPMリポジトリ
https://sayabeans.github.io/vpm/

Booth配布ページ
https://sayamame-beans.booth.pm/items/5057270

GitHubリポジトリ
https://github.com/Sayamame-beans/KiseteNe-for-MA

---

KiseteNe for MAの独自機能について

・調整値の保存/読み込みに対応しています。
KiseteNe for MAで行った調整を保存、共有することが出来ます。

「ファイルに調整値を保存する」
jsonファイルに調整値を出力します。

「ファイルから調整値を(現在の状態として)読み込む」
現在のアバターの状態をファイルの内容が適用されている状態として、KiseteNe for MAの表示と内部状態を揃えます。
これにより、調整を途中から再開することが出来ます。
デフォルト値の再現に誤差が生まれることがあり、RESETを押しても調整前と全く同じ状態にはならない場合があります。

「ファイルから調整値を読み込み、新たに適用する」
現在のアバターの状態をデフォルトの状態として、ファイルの内容をアバターに適用します。

---

「VPAI」について
BOOTH等で配布している本ツールのunitypackageにはVPAIを用いているため、インポートすると自動的にVPMリポジトリからパッケージをダウンロードし、プロジェクトに導入されます。
VPAIでは、VCCのパッケージ一覧にもパッケージが追加されるようになっていますが、2023年8月31日現在ではこちらのバグ( https://github.com/vrchat-community/creator-companion/issues/111 )によって反映されない可能性が高いです。

ちなみに、VPAI Packageは、WebツールかUnityエディタ拡張によって生成することが出来ます。

VPAI
https://github.com/anatawa12/VPMPackageAutoInstaller

Unityエディタ拡張(Booth)
https://anatawa12.booth.pm/items/4951120

---

「キセテネ」について

製作: 灯火/とも屋
https://shivi.booth.pm/

配布ページ
https://shivi.booth.pm/items/2332420

解説ページ
https://tomo-shi-vi.hateblo.jp/entry/kisetene
