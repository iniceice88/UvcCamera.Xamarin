package com.serenegiant.media;
/*
 * libcommon
 * utility/helper classes for myself
 *
 * Copyright (c) 2014-2018 saki t_saki@serenegiant.com
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

import android.media.MediaCodec;
import android.media.MediaFormat;
import android.support.annotation.IntDef;
import android.support.annotation.Nullable;
import android.support.v4.provider.DocumentFile;
import android.view.Surface;

import java.io.IOException;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.nio.ByteBuffer;

public interface IRecorder {

    public interface RecorderCallback {
        public void onPrepared(IRecorder recorder);

        public void onStarted(IRecorder recorder);

        public void onStopped(IRecorder recorder);

        public void onError(Exception e);
    }

    /**
     * キャプチャ終了
     */
//	public static final int STATE_STOPPED = 6;

    @IntDef({RecorderConsts.STATE_UNINITIALIZED,
            RecorderConsts.STATE_INITIALIZED,
            RecorderConsts.STATE_PREPARED,
            RecorderConsts.STATE_STARTING,
            RecorderConsts.STATE_STARTED,
            RecorderConsts.STATE_STOPPING})
    @Retention(RetentionPolicy.SOURCE)
    public @interface RecorderState {
    }

    public void setMuxer(final IMuxer muxer);

    /**
     * Encoderの準備
     * 割り当てられているMediaEncoderの下位クラスのインスタンスの#prepareを呼び出す
     *
     * @throws IOException
     */
    public void prepare();

    /**
     * キャプチャ開始要求
     * 割り当てられているEncoderの下位クラスのインスタンスの#startRecordingを呼び出す
     */
    public abstract void startRecording() throws IllegalStateException;

    /**
     * キャプチャ終了要求
     * 割り当てられているEncoderの下位クラスの#stopRecordingを呼び出す
     */
    public void stopRecording();

    public Surface getInputSurface();

    public Encoder getVideoEncoder();

    public Encoder getAudioEncoder();

    /**
     * Muxerが出力開始しているかどうかを返す
     *
     * @return
     */
    public boolean isStarted();

    /**
     * エンコーダーの初期化が終わって書き込み可能になったかどうかを返す
     *
     * @return
     */
    public boolean isReady();

    /**
     * 終了処理中かどうかを返す
     *
     * @return
     */
    public boolean isStopping();

    /**
     * 終了したかどうかを返す
     *
     * @return
     */
    public boolean isStopped();

    public int getState();

    public IMuxer getMuxer();

    @Nullable
    public String getOutputPath();

    @Nullable
    public DocumentFile getOutputFile();

    public void frameAvailableSoon();

    /**
     * 関連するリソースを開放する
     */
    public void release();

    public void addEncoder(final Encoder encoder);

    public void removeEncoder(final Encoder encoder);

    public boolean start(final Encoder encoder);

    public void stop(final Encoder encoder);

    public int addTrack(final Encoder encoder, final MediaFormat format);

    public void writeSampleData(final int trackIndex,
                                final ByteBuffer byteBuf, final MediaCodec.BufferInfo bufferInfo);
}