//
//  XunFeiSR.mm
//  Unity-iPhone
//
//  Created by SnaplingoMac on 2017/11/2.
//

#import <UIKit/UIKit.h>
#import "iflyMSC/iflyMSC.h"

@interface XunFeiSR :  UIViewController<IFlySpeechRecognizerDelegate,IFlyPcmRecorderDelegate>

@property (nonatomic, strong) IFlySpeechRecognizer* iFlySpeechRecognizer;//不带界面的识别对象
@property (nonatomic,strong) IFlyPcmRecorder *pcmRecorder;//录音器，用于音频流识别的数据传入
@property (nonatomic, strong) NSString * result;
@property (nonatomic, assign) BOOL isCanceled;
+ (XunFeiSR*) instance;
@end

@implementation XunFeiSR
static XunFeiSR* gameMgr=nil;
- (id)init
{
    self = [super init];
    if (self) {
        // Initialization code here.
    }
    
    return self;
}

+ (XunFeiSR*) instance
{
    if (gameMgr==nil)
    {
        gameMgr=[[XunFeiSR alloc]init];
    }
    
    return gameMgr;
}

-(void)initRecognizer
{
    NSLog(@"%s",__func__);
    
    //单例模式，无UI的实例
    if (_iFlySpeechRecognizer == nil) {
        _iFlySpeechRecognizer = [IFlySpeechRecognizer sharedInstance];
        NSLog(@"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        //设置听写模式
        [_iFlySpeechRecognizer setParameter:@"iat" forKey:[IFlySpeechConstant IFLY_DOMAIN]];
        
        [_iFlySpeechRecognizer setParameter:nil forKey:[IFlySpeechConstant ASR_AUDIO_PATH]];
        //设置最长录音时间
        [_iFlySpeechRecognizer setParameter:@"100000" forKey:[IFlySpeechConstant SPEECH_TIMEOUT]];
        //设置后端点
        [_iFlySpeechRecognizer setParameter:@"100000" forKey:[IFlySpeechConstant VAD_EOS]];
        //设置前端点
        [_iFlySpeechRecognizer setParameter:@"100000" forKey:[IFlySpeechConstant VAD_BOS]];
        //网络等待时间
        //[_iFlySpeechRecognizer setParameter:@"5000" forKey:[IFlySpeechConstant NET_TIMEOUT]];
        //设置采样率，推荐使用16K
        [_iFlySpeechRecognizer setParameter:@"16000" forKey:[IFlySpeechConstant SAMPLE_RATE]];
        //设置语言
        [_iFlySpeechRecognizer setParameter:@"en_us" forKey:[IFlySpeechConstant LANGUAGE]];
        [_iFlySpeechRecognizer setParameter:@"" forKey:[IFlySpeechConstant ACCENT]];
        //设置是否返回标点符号
        [_iFlySpeechRecognizer setParameter:@"0" forKey:[IFlySpeechConstant ASR_PTT]];
        
        [_iFlySpeechRecognizer setParameter:@"plain" forKey:[IFlySpeechConstant RESULT_TYPE]];
        
        _iFlySpeechRecognizer.delegate = self;

        if (_pcmRecorder == nil)
        {
            _pcmRecorder = [IFlyPcmRecorder sharedInstance];
        }
        
        _pcmRecorder.delegate = self;
        
        [_pcmRecorder setSample:@"16000"];
        
        [_pcmRecorder setSaveAudioPath:nil];    //不保
    }
}


+(void) startRecognize
{
    [XunFeiSR.instance StartVoice];
}

-(void) StartVoice
{
    [_iFlySpeechRecognizer cancel];
    [_iFlySpeechRecognizer setParameter:@"-1" forKey:@"audio_source"];
    [_iFlySpeechRecognizer setDelegate:self];
    BOOL ret = [_iFlySpeechRecognizer startListening];
    NSLog(@"启动识别服务，startListening");
    if (!ret) {
        NSLog(@"启动识别服务失败，请稍后重试");
    }
    else{
        [_pcmRecorder setDelegate:self];
        [_pcmRecorder start];
    }
}

+(void) stopRecognize
{
    [XunFeiSR.instance stopListening];
}

+(void) cancelListening
{
    [XunFeiSR.instance cancelListening];
}

-(void)cancelListening
{
    [_pcmRecorder stop];
    [_iFlySpeechRecognizer cancel];
}

-(void) stopListening
{
    [_pcmRecorder stop];
    [_iFlySpeechRecognizer stopListening];
}

#pragma mark - IFlySpeechRecognizerDelegate

-(void) onBeginOfSpeech
{
    NSLog(@"on Begin of Speech");
}

//录音结束回调
-(void) onEndOfSpeech
{
    NSLog(@"on End of Speech");
}

-(void) onError:(IFlySpeechError *)error {
    NSLog(@"%s",__func__);
    
    NSString *text ;
    
    if (self.isCanceled) {
        text = @"识别取消";
        
    } else if (error.errorCode == 0 ) {
        if (_result.length == 0) {
            text = @"无识别结果";
        }else {
            text = @"识别成功";
        }
    }else {
        text = [NSString stringWithFormat:@"发生错误：%d %@", error.errorCode,error.errorDesc];
        NSLog(@"%@",text);
    }
 
}

-(void) onResults:(NSArray *)results isLast:(BOOL)isLast {
    NSMutableString *resultString = [[NSMutableString alloc] init];
    NSDictionary *dic = results[0];
    for (NSString *key in dic) {
        [resultString appendFormat:@"%@",key];
    }
    NSString *temp = [[NSString alloc] init];
    _result =[NSString stringWithFormat:@"%@%@", temp,resultString];
    if (isLast){
        NSLog(@"听写结果(json)：%@测试",  self.result);
    }
    NSLog(@"_result=%@",_result);

    const char* str1 = [_result UTF8String];
    UnitySendMessage("XunFeiSRManager","GetResult",str1);
}

#pragma mark - IFlyPcmRecorderDelegate

- (void) onIFlyRecorderBuffer: (const void *)buffer bufferSize:(int)size
{
    NSData *audioBuffer = [NSData dataWithBytes:buffer length:size];
    
    int ret = [_iFlySpeechRecognizer writeAudio:audioBuffer];
    if (!ret)
    {
        NSLog(@"IFLYRecorder error");
        [_iFlySpeechRecognizer stopListening];
    }
}

- (void) onIFlyRecorderError:(IFlyPcmRecorder*)recoder theError:(int) error
{
    
}

@end

extern "C"
{
    void _StartUp(char* id)
    {
        NSLog(@"start up");
        //创建语音配置,appid必须要传入，仅执行一次则可
        NSString *initString = [[NSString alloc] initWithFormat:@"appid=%s",id];
        
        //所有服务启动前，需要确保执行createUtility
        [IFlySpeechUtility createUtility:initString];

        [XunFeiSR.instance initRecognizer];

        [IFlyAudioSession initRecordingAudioSession];
        [IFlySetting showLogcat:false];
    }
    
    void _StartRecognize()
    {
        NSLog(@"start recognize");
        [XunFeiSR.instance StartVoice];
    }
    
    void _StopRecognize()
    {
        NSLog(@"stop recognize");
        [XunFeiSR.instance stopListening];
    }
    
    void _CancelRecognize()
    {
        NSLog(@"cancel recognize");
        [XunFeiSR.instance cancelListening];
    }
}

