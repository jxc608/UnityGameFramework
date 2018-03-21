#import <Foundation/Foundation.h>
#import "WXApi.h"

@interface WXApiManager : NSObject<WXApiDelegate>

+ (instancetype)sharedManager;

- (BOOL)sendImageContent:(NSData*)imageData withThumbImage:(NSData*)thumbData inScene:(int)scene;

@end
