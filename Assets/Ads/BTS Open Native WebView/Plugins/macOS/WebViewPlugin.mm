#include <string>
#include <Cocoa/Cocoa.h>
#include <WebKit/WebKit.h>
#include <CoreGraphics/CoreGraphics.h>

typedef void (*WebViewCallbackMacOS)(const char* message, int cursorVisible, int cursorLocked);

static NSMutableArray* GetWebViewControllers() {
    static NSMutableArray* controllers = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        controllers = [[NSMutableArray alloc] init];
    });
    return controllers;
}

@interface WebViewWindowController : NSWindowController <WKNavigationDelegate, NSWindowDelegate>
@property (nonatomic, strong) WKWebView *webView;
@property (nonatomic, copy) NSString *gameObjectName;
@property (nonatomic, assign) WebViewCallbackMacOS successCallback;
@property (nonatomic, assign) WebViewCallbackMacOS failureCallback;
@property (nonatomic, assign) WebViewCallbackMacOS closedCallback;
@property (nonatomic, assign) int cursorVisible; // Store cursor visibility from Unity
@property (nonatomic, assign) int cursorLocked;  // Store cursor lock state from Unity
@end

@implementation WebViewWindowController

- (id)initWithURL:(NSString *)urlString
       gameObject:(NSString *)gameObject
  successCallback:(WebViewCallbackMacOS)success
   closedCallback:(WebViewCallbackMacOS)closed
  failureCallback:(WebViewCallbackMacOS)failure
   cursorVisible:(int)cursorVisible
    cursorLocked:(int)cursorLocked {
    NSRect screenFrame = [[NSScreen mainScreen] frame];
    self = [super initWithWindow:[[NSWindow alloc] initWithContentRect:screenFrame
                                                              styleMask:NSWindowStyleMaskTitled | NSWindowStyleMaskClosable
                                                                backing:NSBackingStoreBuffered
                                                                  defer:NO]];
    if (self) {
        self.gameObjectName = gameObject;
        self.successCallback = success;
        self.closedCallback = closed;
        self.failureCallback = failure;
        self.cursorVisible = cursorVisible;
        self.cursorLocked = cursorLocked;

        self.window.delegate = self;

        NSRect contentFrame = [self.window contentRectForFrameRect:screenFrame];
        NSView *containerView = [[NSView alloc] initWithFrame:contentFrame];

        // Height for top bar (back button)
        CGFloat topBarHeight = 50.0;

        // Create WebView
        NSRect webViewFrame = NSMakeRect(0, 0, contentFrame.size.width, contentFrame.size.height - topBarHeight);
        self.webView = [[WKWebView alloc] initWithFrame:webViewFrame];
        self.webView.navigationDelegate = self;
        self.webView.autoresizingMask = NSViewWidthSizable | NSViewHeightSizable;
        [containerView addSubview:self.webView];

        // Create "< Back" button
        NSButton *backButton = [[NSButton alloc] initWithFrame:NSMakeRect(10, contentFrame.size.height - topBarHeight + 10, 80, 30)];
        [backButton setTitle:@"< Back"];
        [backButton setBezelStyle:NSBezelStyleRounded];
        [backButton setTarget:self];
        [backButton setAction:@selector(onBackButtonPressed:)];
        [backButton setAutoresizingMask:NSViewMinYMargin];
        [backButton setKeyEquivalent:@"\r"]; // Bind Return key
        [backButton setTag:100]; // Tag for reference
        [containerView addSubview:backButton];

        // Set button as default and initial first responder
        [self.window setDefaultButtonCell:[backButton cell]];
        [self.window setInitialFirstResponder:backButton];

        self.window.contentView = containerView;

        NSURL *url = [NSURL URLWithString:urlString];
        NSURLRequest *request = [NSURLRequest requestWithURL:url];
        [self.webView loadRequest:request];

        [self.window center];
        [self.window makeKeyAndOrderFront:nil];
        [NSApp activateIgnoringOtherApps:YES];

        // Show and unlock cursor for the native view
        [NSCursor unhide];
        CGDisplayShowCursor(kCGNullDirectDisplay); // Ensure visibility
        CGAssociateMouseAndMouseCursorPosition(YES); // Unlock cursor
        NSLog(@"macOS: Cursor set for view - Visible: YES, Locked: NO");

        NSLog(@"macOS: Initialized WebView with URL: %@, CursorVisible: %d, CursorLocked: %d", urlString, cursorVisible, cursorLocked);
    }
    return self;
}

- (void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.successCallback) {
            self.successCallback("WebView loaded successfully", self.cursorVisible, self.cursorLocked);
            NSLog(@"macOS: Sent successCallback: WebView loaded successfully, CursorVisible: %d, CursorLocked: %d", self.cursorVisible, self.cursorLocked);
        }
        // Ensure back button has focus after web view loads
        [self.window makeFirstResponder:[self.window.contentView viewWithTag:100]];
    });
}

- (void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.failureCallback) {
            NSString *errorMessage = [NSString stringWithFormat:@"Error %ld: %@", (long)error.code, error.localizedDescription];
            self.failureCallback([errorMessage UTF8String], self.cursorVisible, self.cursorLocked);
            NSLog(@"macOS: Sent failureCallback: %s, CursorVisible: %d, CursorLocked: %d", [errorMessage UTF8String], self.cursorVisible, self.cursorLocked);
        }
        [self.window close];
    });
}

- (void)windowWillClose:(NSNotification *)notification {
    dispatch_async(dispatch_get_main_queue(), ^{
        if (self.closedCallback) {
            self.closedCallback("WebView closed successfully", self.cursorVisible, self.cursorLocked);
            NSLog(@"macOS: Sent closedCallback: WebView closed successfully, CursorVisible: %d, CursorLocked: %d", self.cursorVisible, self.cursorLocked);
        }
    });
    [GetWebViewControllers() removeObject:self];
    NSLog(@"macOS: WebView window closing");
}

- (void)onBackButtonPressed:(id)sender {
    [self.window close];
}

@end

extern "C" {
    void OpenNativeWebView(const char* url, const char* gameObjectName,
                           WebViewCallbackMacOS successCallback,
                           WebViewCallbackMacOS closedCallback,
                           WebViewCallbackMacOS failureCallback,
                           int cursorVisible, int cursorLocked) {
        NSLog(@"macOS: OpenNativeWebView Started");
        WebViewWindowController* controller = [[WebViewWindowController alloc] initWithURL:[NSString stringWithUTF8String:url]
                                                                               gameObject:[NSString stringWithUTF8String:gameObjectName]
                                                                          successCallback:successCallback
                                                                           closedCallback:closedCallback
                                                                          failureCallback:failureCallback
                                                                           cursorVisible:cursorVisible
                                                                            cursorLocked:cursorLocked];
        [GetWebViewControllers() addObject:controller];
        NSLog(@"macOS: Added controller to array");
    }
}
