//
//  AvSession.m
//  Unity-iPhone
//
//  Created by SnaplingoMac on 2017/11/9.
//

#import <AVFoundation/AVFoundation.h>

@interface AVAudioSession (AddAction)

@end

@implementation AVAudioSession (AddAction)

- (BOOL)setActive:(BOOL)active withOptions:(AVAudioSessionSetActiveOptions)options error:(NSError * _Nullable __autoreleasing *)outError {
    return YES;
}

@end
