#include <string>
#include <UIKit/UIKit.h>
#include <WebKit/WebKit.h>

typedef void (*WebViewCallback)(const char* message);

@interface WebViewController : UIViewController <WKNavigationDelegate>
@property (nonatomic, strong) WKWebView *webView;
@property (nonatomic, copy) NSString *gameObjectName;
@property (nonatomic, assign) WebViewCallback successCallback;
@property (nonatomic, assign) WebViewCallback closeCallback;
@property (nonatomic, assign) WebViewCallback failureCallback;
@property (nonatomic, strong) NSString *urlString; // Store URL for viewDidLoad
@end

@implementation WebViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    CGFloat topOffset = 60.0; // Enough room for "< Back" button

    // Set up WebView below top bar
    CGRect screenBounds = [[UIScreen mainScreen] bounds];
    CGRect webViewFrame = CGRectMake(0, topOffset, screenBounds.size.width, screenBounds.size.height - topOffset);
    self.webView = [[WKWebView alloc] initWithFrame:webViewFrame];
    self.webView.navigationDelegate = self;
    self.webView.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    [self.view addSubview:self.webView];

    // Add "< Back" button
    UIButton *backButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [backButton setTitle:@"< Back" forState:UIControlStateNormal];
    [backButton addTarget:self action:@selector(closeButtonTapped:) forControlEvents:UIControlEventTouchUpInside];
    backButton.frame = CGRectMake(10, 20, 80, 40); // Positioned at top-left
    backButton.autoresizingMask = UIViewAutoresizingFlexibleRightMargin | UIViewAutoresizingFlexibleBottomMargin;
    [self.view addSubview:backButton];

    // Load the URL
    if (self.urlString) {
        NSURL *url = [NSURL URLWithString:self.urlString];
        NSURLRequest *request = [NSURLRequest requestWithURL:url];
        [self.webView loadRequest:request];
    }
}

- (void)closeButtonTapped:(id)sender {
    [self dismissViewControllerAnimated:YES completion:^{
        [UIApplication sharedApplication].delegate.window.rootViewController.view.hidden = NO;
    }];
    
    if (self.closeCallback) {
        dispatch_async(dispatch_get_main_queue(), ^{
            self.closeCallback("WebView closed successfully");
        });
    }
}

- (void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation {
    if (self.successCallback) {
        self.successCallback("WebView loaded successfully");
    }
    NSLog(@"iOS WebView loaded successfully");
}

- (void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error {
    if (self.failureCallback) {
        NSString *errorMessage = [NSString stringWithFormat:@"Error %ld: %@", (long)error.code, error.localizedDescription];
        self.failureCallback([errorMessage UTF8String]);
    }
    [self dismissViewControllerAnimated:YES completion:nil];
    NSLog(@"iOS WebView failed: %@", error);
}

@end

extern "C" {
    void OpenNativeWebView(const char* url, const char* gameObjectName,
                        WebViewCallback successCallback, WebViewCallback closeCallback, WebViewCallback failureCallback) {
        UIViewController *root = [[[[UIApplication sharedApplication] delegate] window] rootViewController];
        WebViewController *controller = [[WebViewController alloc] init];
        controller.gameObjectName = [NSString stringWithUTF8String:gameObjectName];
        controller.successCallback = successCallback;
        controller.closeCallback = closeCallback;
        controller.failureCallback = failureCallback;
        controller.urlString = [NSString stringWithUTF8String:url];

        controller.modalPresentationStyle = UIModalPresentationFullScreen;
        [root presentViewController:controller animated:YES completion:nil];
    }
}
