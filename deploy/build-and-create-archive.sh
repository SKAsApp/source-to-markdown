#!/usr/bin/env zsh

set -eu

APP_NAME="source-to-markdown"
SCRIPT_DIRECTORY="$(cd $(dirname ${BASH_SOURCE:-$0}); pwd)"
TEMPORARY_DIRECTORY="${HOME}/Temporary"
VERSION="1.0.0"
VERSION_STR="1_0_0"


RUNTIMES=(
	"linux-x64"
	"linux-arm64"
	"osx-arm64"
	"osx-x64"
	"win-arm64"
	"win-x64"
)

# 1/5 ビルド
echo -e "\033[36m1/5 ビルド 開始\033[0m"
rm -f -R "${SCRIPT_DIRECTORY}/../bin/Release"

for runtime in ${RUNTIMES}
do
	echo ""
	echo "${runtime}ビルド開始"
	dotnet publish "./source-to-markdown.csproj" --configuration "Release" --runtime "${runtime}"
done

# 2/5 一時ディレクトリーにコピー
WORK_DIRECTORY="${TEMPORARY_DIRECTORY}/${APP_NAME}"
echo ""
echo -e "\033[36m2/5 一時ディレクトリー「${WORK_DIRECTORY}」にコピー 開始\033[0m"
mkdir -p "${WORK_DIRECTORY}"
for runtime in ${RUNTIMES}
do
	cp -R "./bin/Release/net10.0/" "${WORK_DIRECTORY}"
done

# 3/5 権限設定とアーカイブの作成
echo ""
echo -e "\033[36m3/5 一時ディレクトリーの権限設定とアーカイブの作成 開始\033[0m"
cd "${WORK_DIRECTORY}"

for runtime in ${RUNTIMES}
do
	echo ""
	echo "${runtime}処理開始"
	if [ ! -d "./${runtime}" ]
	then
		echo "ディレクトリが存在しないためスキップします: ./${runtime}" >&2
		continue
	fi

	if [ ! -d "./${runtime}/publish" ]
	then
		echo "配布対象ディレクトリが存在しないためスキップします: ./${runtime}/${APP_NAME}" >&2
		continue
	fi

	# Windowsでは実行ファイルに拡張子「.exe」が付くため、調整
	executable_file_name="${APP_NAME}"
	if [ "${runtime}" = "win-arm64" ] || [ "${runtime}" = "win-x64" ]
	then
		executable_file_name="${APP_NAME}.exe"
	fi
	# ファイル入出力まわりの変数定義
	tar_file="./${APP_NAME}-${VERSION_STR}-${runtime}.tar"
	tar_xz_file="../${APP_NAME}-${VERSION_STR}-${runtime}.tar.xz"
	# macOSは独自のファイル名へ
	if [ "${runtime}" = "osx-arm64" ]
	then
		tar_file="./${APP_NAME}-${VERSION_STR}-macos-apple-silicon.tar"
		tar_xz_file="../${APP_NAME}-${VERSION_STR}-macos-apple-silicon.tar.xz"
	fi
	if [ "${runtime}" = "osx-x64" ]
	then
		tar_file="./${APP_NAME}-${VERSION_STR}-macos-intel.tar"
		tar_xz_file="../${APP_NAME}-${VERSION_STR}-macos-intel.tar.xz"
	fi
	# linux-x64はamd64
	if [ "${runtime}" = "linux-x64" ]
	then
		tar_file="./${APP_NAME}-${VERSION_STR}-linux-amd64.tar"
		tar_xz_file="../${APP_NAME}-${VERSION_STR}-linux-amd64.tar.xz"
	fi
	# winはwindows
	if [ "${runtime}" = "win-arm64" ]
	then
		tar_file="./${APP_NAME}-${VERSION_STR}-windows-arm64.tar"
		tar_xz_file="../${APP_NAME}-${VERSION_STR}-windows-arm64.tar.xz"
	fi
	if [ "${runtime}" = "win-x64" ]
	then
		tar_file="./${APP_NAME}-${VERSION_STR}-windows-amd64.tar"
		tar_xz_file="../${APP_NAME}-${VERSION_STR}-windows-amd64.tar.xz"
	fi
	executable_file_path="./${APP_NAME}/${executable_file_name}"

	# 処理開始
	cd "./${runtime}"
	# アーカイブ対象のディレクトリー名変更
	rm -f -R "./${APP_NAME}"
	mv "./publish" "./${APP_NAME}"
	if [ ! -f "${executable_file_path}" ]
	then
		echo "実行ファイルが存在しません: ./${runtime}/${executable_file_path}" >&2
		cd ../
		continue
	fi
	# 既存ファイル削除
	rm -f "${tar_file}" "${tar_xz_file}"
	# 権限設定
	find "./${APP_NAME}" -type f -exec chmod 644 {} \;
	find "./${APP_NAME}" -type d -exec chmod 755 {} \;
	chmod 755 "${executable_file_path}"

	# アーカイブ化
	tar -c --owner=0 --group=0 -f "${tar_file}" "./${APP_NAME}"
	chmod 644 "${tar_file}"

	# xz圧縮
	pixz -9 -p 7 "${tar_file}" "${tar_xz_file}"
	cd ../
	echo "アーカイブ作成：${tar_xz_file}"
done

# 4/5 チェックサムの作成
echo ""
echo -e "\033[36m4/5 チェックサムの作成 開始\033[0m"
sha256sum "./${APP_NAME}-${VERSION_STR}-macos-apple-silicon.tar.xz" "./${APP_NAME}-${VERSION_STR}-macos-intel.tar.xz" "./${APP_NAME}-${VERSION_STR}-linux-amd64.tar.xz" "./${APP_NAME}-${VERSION_STR}-linux-arm64.tar.xz" "./${APP_NAME}-${VERSION_STR}-windows-amd64.tar.xz" "./${APP_NAME}-${VERSION_STR}-windows-arm64.tar.xz" > "./SHA256SUMS.txt"

# 5/5 元のディレクトリーにコピー
echo ""
echo -e "\033[36m5/5 元のディレクトリーにコピー 開始\033[0m"
mv "./${APP_NAME}-${VERSION_STR}-macos-apple-silicon.tar.xz" "${SCRIPT_DIRECTORY}/../bin"
mv "./${APP_NAME}-${VERSION_STR}-macos-intel.tar.xz" "${SCRIPT_DIRECTORY}/../bin"
mv "./${APP_NAME}-${VERSION_STR}-linux-amd64.tar.xz" "${SCRIPT_DIRECTORY}/../bin"
mv "./${APP_NAME}-${VERSION_STR}-linux-arm64.tar.xz" "${SCRIPT_DIRECTORY}/../bin"
mv "./${APP_NAME}-${VERSION_STR}-windows-amd64.tar.xz" "${SCRIPT_DIRECTORY}/../bin"
mv "./${APP_NAME}-${VERSION_STR}-windows-arm64.tar.xz" "${SCRIPT_DIRECTORY}/../bin"
mv "./SHA256SUMS.txt" "${SCRIPT_DIRECTORY}/../bin"
# 一時ディレクトリーの削除
cd "${SCRIPT_DIRECTORY}/../"
rm -f -R "${WORK_DIRECTORY}"

echo "すべての処理が完了しました。"
