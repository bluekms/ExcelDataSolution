# Sdp 日本語ドキュメント

Excel データを C# レコードに検証・ロードし、メモリ上で高速に照会するパイプラインライブラリ **StaticDataPipeline (Sdp)** の日本語ドキュメントです。

> このドキュメントは、信頼できる原典である韓国語版から AI によって機械翻訳されたものです。

## クイックスタート

初めてなら **[クイックスタート](./quickstart.md)** から — 5 分で Record 定義 → ロード → 照会まで終わらせる 1 ページです。Sdp がどんな問題を解き、どんな流れで動作するのかは [1. 紹介](./01-introduction.md) で扱います。

</br></br></br>

## 目次

### 1. [紹介](./01-introduction.md)
Sdp が解決する問題、主な利点、データフロー。

### 2. [インストール](./02-installation.md)
要求環境とインストール方法。

### 3. 使い方
例で学ぶパイプライン。一部の章は **データ作業者** が Excel を埋めるときに、残りは **レコード作業者** が Record/Table/Manager を構成するときに開いて見る案内です。データ構造が合意された後は、2 つの作業を互いに待たずに並列で進められます。
- [3.1 Excel を扱う](./03-usage/01-record-to-excel.md) — データ作業者の視点
- [3.2 標準ヘッダージェネレーター](./03-usage/02-header-generator.md) — データ作業者の視点
- [3.3 最初の Record を定義する](./03-usage/03-first-record.md) — レコード作業者の視点
- [3.4 StaticDataTable の実装](./03-usage/04-static-data-table.md)
- [3.5 StaticDataManager で複数テーブルを管理](./03-usage/05-static-data-manager.md)
- [3.6 外部キー (ForeignKey, SwitchForeignKey)](./03-usage/06-foreign-keys.md)
- [3.7 StaticDataView 事前生成ビュー](./03-usage/07-static-data-view.md)

### 4. CLI ツール
ビルドパイプラインから呼び出す 2 つの CLI ツールの使い方 — コマンドの形、オプション全体、bat の例。
- [4.1 StaticDataHeaderGenerator](./04-cli-tools/01-header-generator.md)
- [4.2 ExcelColumnExtractor](./04-cli-tools/02-column-extractor.md)

### 5. 高度な機能
- [5.1 サポート型 (Schemata)](./05-advanced/01-schemata.md)
- [5.2 Attribute カタログ](./05-advanced/02-attributes.md)
- [5.3 タイプブランディングパターン](./05-advanced/03-type-branding.md)
- [5.4 バリデーション概要](./05-advanced/04-validation.md)

### 6. [ライセンス](./06-license.md)

---

[クイックスタート](./quickstart.md) で一度最後まで動かしてみたうえで、詰まった箇所から該当の章へ戻る流れをお勧めします。じっくり読むなら、役割に合わせて次のように追っていくと自然です。

- **データ作業者**: 1 → 3.1 → 3.2 — Excel の作成に必要な内容はこの 2 章で十分です。
- **レコード作業者**: 1 → 3.3 から最後まで — Record/Table/Manager/View の構成と CLI ツール (4 章)、高度な機能 (5 章) まで全部目を通しておくとよいでしょう。
