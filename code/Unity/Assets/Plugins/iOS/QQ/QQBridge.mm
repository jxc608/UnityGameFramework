#import <TencentOpenAPI/TencentOAuth.h>
#import <TencentOpenAPI/QQApiInterface.h>
#import <TencentOpenAPI/QQApiInterfaceObject.h>

extern "C"
{
    void _RegisterAppQQ(const char* appId)
    {
        [[TencentOAuth alloc] initWithAppId:[NSString stringWithUTF8String:appId] andDelegate:nil];
    }
    
    
    void _ShareImageQQ(Byte* ptr, int size, Byte* ptrThumb, int sizeThumb)
    {
        NSData *data = [[NSData alloc] initWithBytes:ptr length:size];
        NSData *dataThumb = [[NSData alloc] initWithBytes:ptrThumb length:sizeThumb];
        QQApiImageObject *imgObj = [QQApiImageObject objectWithData:data
                                                   previewImageData:dataThumb
                                                              title:@""
                                                        description:@""];
        imgObj.shareDestType = ShareDestTypeQQ;
        SendMessageToQQReq *req = [SendMessageToQQReq reqWithContent:imgObj];
        [QQApiInterface sendReq:req];
    }
    
    void _ShareImageQzone(Byte* ptr, int size, const char* summary)
    {
        NSData *data = [[NSData alloc] initWithBytes:ptr length:size];
        NSArray *array = [NSArray arrayWithObject:data];
        QQApiImageArrayForQZoneObject *imgObj = [QQApiImageArrayForQZoneObject objectWithimageDataArray:array title:[NSString stringWithUTF8String:summary]];
        imgObj.shareDestType = ShareDestTypeQQ;
        SendMessageToQQReq *req = [SendMessageToQQReq reqWithContent:imgObj];
        [QQApiInterface SendReqToQZone:req];
    }
    
    bool _IsQQInstalled()
    {
        return [QQApiInterface isQQInstalled];
    }
    
    bool _IsQQAppSupportApi()
    {
        return [QQApiInterface isQQSupportApi];
    }
    
}

