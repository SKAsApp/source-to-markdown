#!/bin/sh
# Copilot作成

set -eu

APP_NAME="source-to-markdown"

RUNTIMES="
linux-x64
linux-arm64
osx-arm64
osx-x64
win-arm64
win-x64
"

for runtime in ${RUNTIMES}
do
	echo "処理開始: ${runtime}"

	if [ ! -d "./${runtime}" ]
	then
		echo "ディレクトリが存在しないためスキップします: ./${runtime}" >&2
		continue
	fi

	if [ ! -d "./${runtime}/${APP_NAME}" ]
	then
		echo "配布対象ディレクトリが存在しないためスキップします: ./${runtime}/${APP_NAME}" >&2
		continue
	fi

	executable_file_name="${APP_NAME}"

	case "${runtime}" in
		win-arm64|win-x64)
			executable_file_name="${APP_NAME}.exe"
			;;
	esac

	cd "./${runtime}"

	tar_file="./${APP_NAME}-${runtime}.tar"
	tar_xz_file="../${APP_NAME}-${runtime}.tar.xz"
	executable_file_path="./${APP_NAME}/${executable_file_name}"

	rm -f "${tar_file}" "${tar_xz_file}"

	find "./${APP_NAME}" -type f -exec chmod 644 {} \;
	find "./${APP_NAME}" -type d -exec chmod 755 {} \;

	if [ ! -f "${executable_file_path}" ]
	then
		echo "実行ファイルが存在しません: ./${runtime}/${executable_file_path}" >&2
		cd ../
		continue
	fi

	chmod 755 "${executable_file_path}"

	tar -c --owner=0 --group=0 -f "${tar_file}" "./${APP_NAME}"

	chmod 644 "${tar_file}"

	pixz -9 -p 7 "${tar_file}" "${tar_xz_file}"

	cd ../

	echo "処理完了: ${APP_NAME}-${runtime}.tar.xz"
done

echo "すべての処理が完了しました。"
