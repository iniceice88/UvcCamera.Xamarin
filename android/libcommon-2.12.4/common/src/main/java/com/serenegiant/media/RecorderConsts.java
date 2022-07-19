package com.serenegiant.media;

public final class RecorderConsts {

    /**
     * キャプチャしていない
     */
    public static final int STATE_UNINITIALIZED = 0;
    /**
     * キャプチャ初期化済(Muxerセット済)
     */
    public static final int STATE_INITIALIZED = 1;
    /**
     * キャプチャ準備完了(prepare済)
     */
    public static final int STATE_PREPARED = 2;
    /**
     * キャプチャ開始中
     */
    public static final int STATE_STARTING = 3;
    /**
     * キャプチャ中
     */
    public static final int STATE_STARTED = 4;
    /**
     * キャプチャ停止要求中
     */
    public static final int STATE_STOPPING = 5;
}