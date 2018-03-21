//
//  AlbumUtils.m
//  Unity-iPhone
//
//  Created by mack on 2017/4/28.
//
//

@interface AlbumUtils : NSObject

@end

@implementation AlbumUtils

+(void) saveImageToPhotosAlbum:(NSData*)data
{
    UIImageWriteToSavedPhotosAlbum([UIImage imageWithData:data], self, @selector(image:didFinishSavingWithError:contextInfo:), nil);
}

+(void) image:(UIImage*)image didFinishSavingWithError:(NSError*)error contextInfo:(void*)contextInfo
{
    NSString* result;
    if(error) {
        result = @"false";
    } else {
        result = @"true";
    }
    UnitySendMessage( "AlbumManager", "SaveImageToAlbumCallback", [result UTF8String]);
}

@end

extern "C"
{
    void _SaveImageToAlbum(Byte* ptr, int size)
    {
        [AlbumUtils saveImageToPhotosAlbum:[[NSData alloc] initWithBytes:ptr length:size]];
    }
}
