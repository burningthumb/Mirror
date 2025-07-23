// ImageViewController.mm
#include <string>
#include <UIKit/UIKit.h>

// Define callback type for Unity integration
typedef void (*ImageViewCallback)(const char* message);

// Static window to manage the image view
static UIWindow *imageWindow = nil;

// Objective-C++ class for the image view controller
@interface ImageViewController : UIViewController
@property (nonatomic, strong) UIImageView *imageView;
@property (nonatomic, strong) UIButton *closeButton;
@property (nonatomic, copy) NSString *gameObjectName;
@property (nonatomic, assign) ImageViewCallback successCallback;
@property (nonatomic, assign) ImageViewCallback closeCallback;
@property (nonatomic, assign) ImageViewCallback failureCallback;
@property (nonatomic, strong) NSString *imageUrlString;
@end

@implementation ImageViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    // Set up full-screen view
    self.view.backgroundColor = [UIColor blackColor];
    
    // Initialize full-screen image view
    self.imageView = [[UIImageView alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
    self.imageView.contentMode = UIViewContentModeScaleAspectFit;
    self.imageView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    [self.view addSubview:self.imageView];

    // Add Close button ("< Back")
    self.closeButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [self.closeButton setTitle:@"< Back" forState:UIControlStateNormal];
    [self.closeButton setTitleColor:[UIColor blackColor] forState:UIControlStateNormal];
    [self.closeButton addTarget:self action:@selector(closeButtonTapped:) forControlEvents:UIControlEventPrimaryActionTriggered];
    self.closeButton.frame = CGRectMake(50, 50, 200, 50);
    self.closeButton.userInteractionEnabled = YES;
    self.closeButton.enabled = YES;
    [self.view addSubview:self.closeButton];

    // Ensure focus for tvOS
    [self setNeedsFocusUpdate];
    
    // Load the image
    [self loadImageFromURL];
}

- (void)loadImageFromURL {
    NSURL *url = [NSURL URLWithString:self.imageUrlString];
    NSURLSession *session = [NSURLSession sharedSession];
    NSURLSessionDataTask *task = [session dataTaskWithURL:url completionHandler:^(NSData *data, NSURLResponse *response, NSError *error) {
        if (error || !data) {
            dispatch_async(dispatch_get_main_queue(), ^{
                if (self.failureCallback) {
                    self.failureCallback("Failed to load image");
                }
                NSLog(@"tvOS: Failed to load image: %@", error);
            });
            return;
        }

        UIImage *image = [UIImage imageWithData:data];
        dispatch_async(dispatch_get_main_queue(), ^{
            if (image) {
                self.imageView.image = image;
                if (self.successCallback) {
                    self.successCallback("Image loaded successfully");
                }
                NSLog(@"tvOS: Image loaded successfully");
            } else {
                if (self.failureCallback) {
                    self.failureCallback("Invalid image data");
                }
                NSLog(@"tvOS: Invalid image data");
            }
        });
    }];
    [task resume];
}

- (void)closeButtonTapped:(id)sender {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.closeCallback) {
            self.closeCallback("ImageView closed successfully");
        }
    });
    
    NSLog(@"tvOS: Close button tapped");
    imageWindow.hidden = YES;
    imageWindow = nil;
    NSLog(@"tvOS: Image window hidden");
    
    // Restore Unity’s window
    UIWindow *unityWindow = [UIApplication sharedApplication].keyWindow;
    unityWindow.hidden = NO;
    [unityWindow makeKeyAndVisible];
    
    NSLog(@"tvOS: Image window no longer needed. Releasing it");
    imageWindow = nil;
}

// tvOS focus support
- (NSArray<id<UIFocusEnvironment>> *)preferredFocusEnvironments {
    NSLog(@"tvOS: Setting preferred focus to closeButton");
    return @[self.closeButton];
}

- (void)didUpdateFocusInContext:(UIFocusUpdateContext *)context withAnimationCoordinator:(UIFocusAnimationCoordinator *)coordinator {
    NSLog(@"tvOS: Focus updated to %@", context.nextFocusedView);
}

// Handle Apple TV remote Menu button
- (void)pressesBegan:(NSSet<UIPress *> *)presses withEvent:(UIPressesEvent *)event {
    for (UIPress *press in presses) {
        if (press.type == UIPressTypeMenu) {
            NSLog(@"tvOS: Menu button pressed");
            [self closeButtonTapped:nil];
            return;
        }
    }
    [super pressesBegan:presses withEvent:event];
}

@end

// C-style interface for Unity
extern "C" {
    void OpenNativeImageView(const char* url, const char* gameObjectName,
                             ImageViewCallback successCallback, ImageViewCallback closeCallback, ImageViewCallback failureCallback) {
        if (imageWindow) {
//            NSLog(@"tvOS: Image window already exists, reusing it");
//            imageWindow.hidden = NO;
//            [imageWindow makeKeyAndVisible];
//            return;
            
            NSLog(@"tvOS: Image window already exists, releasing it");
            imageWindow = nil;
        }

        // Create a new image window
        imageWindow = [[UIWindow alloc] initWithFrame:[[UIScreen mainScreen] bounds]];
        ImageViewController *controller = [[ImageViewController alloc] init];
        controller.gameObjectName = [NSString stringWithUTF8String:gameObjectName];
        controller.successCallback = successCallback;
        controller.closeCallback = closeCallback;
        controller.failureCallback = failureCallback;
        controller.imageUrlString = [NSString stringWithUTF8String:url];
        
        imageWindow.rootViewController = controller;
        imageWindow.windowLevel = UIWindowLevelNormal;
        [imageWindow makeKeyAndVisible];
        
        NSLog(@"tvOS: Created and presented image window");
    }
}

// C-style interface for Unity
extern "C" {
    void OpenNativeWebView(const char* url, const char* gameObjectName,
                             ImageViewCallback successCallback, ImageViewCallback closeCallback, ImageViewCallback failureCallback) {
        
        OpenNativeImageView(url, gameObjectName,
                            successCallback, closeCallback, failureCallback);
    }
}
