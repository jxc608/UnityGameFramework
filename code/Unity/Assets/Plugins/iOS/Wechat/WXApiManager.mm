#import "WXApiManager.h"

@implementation WXApiManager

#pragma mark - LifeCycle
+(instancetype)sharedManager {
    static dispatch_once_t onceToken;
    static WXApiManager *instance;
    dispatch_once(&onceToken, ^{
        instance = [[WXApiManager alloc] init];
    });
    return instance;
}

- (void)dealloc {
}

- (BOOL)sendImageContent:(NSData*)imageData withThumbImage:(NSData*)thumbData inScene:(int)scene {
    WXImageObject *ext = [WXImageObject object];
    ext.imageData = imageData;
    
    WXMediaMessage *message = [WXMediaMessage message];
    message.title = @"title";
    message.description = @"desc";
    message.mediaObject = ext;
    message.messageExt = @"ext";
    message.messageAction = @"action";
    message.mediaTagName = @"tag";
    [message setThumbImage:[UIImage imageWithData:thumbData]];
    
    SendMessageToWXReq *req = [[SendMessageToWXReq alloc] init];
    req.bText = false;
    req.scene = scene;
    req.message = message;
    
    return [WXApi sendReq:req];
}

#pragma mark - WXApiDelegate
- (void)onResp:(BaseResp *)resp {
    if ([resp isKindOfClass:[SendMessageToWXResp class]]) {
        SendMessageToWXResp *messageResp = (SendMessageToWXResp *)resp;
        NSLog(@"errcode: %d", messageResp.errCode);
        UnitySendMessage("ShareManager", "WechatCallBack", [[NSString stringWithFormat:@"%d", messageResp.errCode] UTF8String]);
    } else if ([resp isKindOfClass:[SendAuthResp class]]) {
    } else if ([resp isKindOfClass:[AddCardToWXCardPackageResp class]]) {
    } else if ([resp isKindOfClass:[WXChooseCardResp class]]) {
    }
}

- (void)onReq:(BaseReq *)req {
    if ([req isKindOfClass:[GetMessageFromWXReq class]]) {
    } else if ([req isKindOfClass:[ShowMessageFromWXReq class]]) {
    } else if ([req isKindOfClass:[LaunchFromWXReq class]]) {
    }
}

@end

extern "C"
{
    void _RegisterAppWechat(const char* appId)
    {
        [WXApi registerApp:[NSString stringWithUTF8String:appId]];
        //向微信注册支持的文件类型
        UInt64 typeFlag = MMAPP_SUPPORT_TEXT | MMAPP_SUPPORT_PICTURE | MMAPP_SUPPORT_LOCATION | MMAPP_SUPPORT_VIDEO |MMAPP_SUPPORT_AUDIO | MMAPP_SUPPORT_WEBPAGE | MMAPP_SUPPORT_DOC | MMAPP_SUPPORT_DOCX | MMAPP_SUPPORT_PPT | MMAPP_SUPPORT_PPTX | MMAPP_SUPPORT_XLS | MMAPP_SUPPORT_XLSX | MMAPP_SUPPORT_PDF;
        
        [WXApi registerAppSupportContentFlag:typeFlag];
    }
    
    
    void _ShareImageWechat(int scene, Byte* ptr, int size, Byte* ptrThumb, int sizeThumb)
    {
        NSData *data = [[NSData alloc] initWithBytes:ptr length:size];
        NSData *dataThumb = [[NSData alloc] initWithBytes:ptrThumb length:sizeThumb];
        [[WXApiManager sharedManager] sendImageContent:data withThumbImage:dataThumb inScene:scene];
    }
    
    bool _IsWechatInstalled()
    {
        return [WXApi isWXAppInstalled];
    }
    
    bool _IsWechatAppSupportApi() {
        return [WXApi isWXAppSupportApi];
    }
    
}

