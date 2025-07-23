package com.burningthumb.mobile.webview;

import android.app.Activity;
import android.content.ActivityNotFoundException;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.AsyncTask;
import android.view.KeyEvent;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import com.unity3d.player.UnityPlayer;

import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;

public class WebViewPlugin {
    private static WebView webView;
    private static ImageView imageView;
    private static ViewGroup mainLayout; // Supports LinearLayout and FrameLayout
    private static Activity unityActivity;
    private static String gameObjectName;

    private static String successCallback;
    private static String closeCallback;
    private static String failureCallback;

    // Opens a native WebView with the specified URL
    public static void openNativeWebView(final Activity activity, final String url, final String unityGameObjectName,
                                         final String successCallback, final String closeCallback, final String failureCallback) {
        unityActivity = activity;
        gameObjectName = unityGameObjectName;

        WebViewPlugin.successCallback = successCallback;
        WebViewPlugin.closeCallback = closeCallback;
        WebViewPlugin.failureCallback = failureCallback;

        activity.runOnUiThread(() -> {
            if (webView != null) {
                webView.setVisibility(View.VISIBLE);
                focusView();
                pauseUnity();
                return;
            }

            setupMainLayout(activity);
            LinearLayout navBar = createNavBar(activity);
            webView = new WebView(activity);
            webView.setLayoutParams(new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                0,
                1f
            ));
            webView.getSettings().setJavaScriptEnabled(true);

            webView.setWebViewClient(new WebViewClient() {
                @Override
                public void onPageFinished(WebView view, String url) {
                    UnityPlayer.UnitySendMessage(gameObjectName, successCallback, "WebView loaded successfully");
                }

                @Override
                public void onReceivedError(WebView view, int errorCode, String description, String failingUrl) {
                    UnityPlayer.UnitySendMessage(gameObjectName, failureCallback, "Failed to load WebView: " + description);
                }

                @Override
                public boolean shouldOverrideUrlLoading(WebView view, String url) {
                    if (url.startsWith("intent://")) {
                        try {
                            Intent intent = Intent.parseUri(url, Intent.URI_INTENT_SCHEME);
                            activity.startActivity(intent);
                            UnityPlayer.UnitySendMessage(gameObjectName, successCallback, "Play Store app opened");
                            closeView();
                            return true;
                        } catch (ActivityNotFoundException e) {
                            UnityPlayer.UnitySendMessage(gameObjectName, failureCallback, "Play Store app not found");
                            return false;
                        } catch (Exception e) {
                            UnityPlayer.UnitySendMessage(gameObjectName, failureCallback, "Failed to parse intent: " + e.getMessage());
                            return false;
                        }
                    }
                    view.loadUrl(url);
                    return true;
                }
            });

            mainLayout.addView(navBar);
            mainLayout.addView(webView);

            activity.addContentView(mainLayout, new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            ));

            if (url != null && !url.isEmpty()) {
                webView.loadUrl(url);
                focusView();
                pauseUnity();
            } else {
                UnityPlayer.UnitySendMessage(gameObjectName, failureCallback, "No URL provided");
                closeView();
            }
        });
    }

    // Opens a native ImageView with the specified image URL
    public static void openNativeImageView(final Activity activity, final String imageUrl, final String unityGameObjectName,
                                           final String successCallback, final String closeCallback, final String failureCallback) {
        unityActivity = activity;
        gameObjectName = unityGameObjectName;

        WebViewPlugin.successCallback = successCallback;
        WebViewPlugin.closeCallback = closeCallback;
        WebViewPlugin.failureCallback = failureCallback;

        activity.runOnUiThread(() -> {
            if (imageView != null && mainLayout != null) {
                mainLayout.setVisibility(View.VISIBLE);
                focusView();
                pauseUnity();
                return;
            }

            // Use FrameLayout for layering views (ImageView behind, Button on top)
            mainLayout = new FrameLayout(activity) {
                @Override
                public boolean dispatchKeyEvent(KeyEvent event) {
                    if (event.getAction() == KeyEvent.ACTION_DOWN && event.getKeyCode() == KeyEvent.KEYCODE_BACK) {
                        closeView();
                        return true;
                    }
                    return super.dispatchKeyEvent(event);
                }
            };
            mainLayout.setLayoutParams(new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            ));
            mainLayout.setBackgroundColor(0xFF000000); // Black background

            // Full-screen ImageView
            imageView = new ImageView(activity);
            imageView.setLayoutParams(new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            ));
            imageView.setScaleType(ImageView.ScaleType.FIT_CENTER);

            // Back button with explicit styling for visibility
            Button backButton = new Button(activity);
            backButton.setText("< Back");
            backButton.setTextColor(0xFF000000); // Black text
            backButton.setBackgroundColor(0xFFFFFFFF); // Solid white background
            backButton.setTextSize(14); // Reduced to 14sp to fit button and match tvOS
            backButton.setPadding(dpToPx(10, activity), dpToPx(5, activity), dpToPx(10, activity), dpToPx(5, activity));
            backButton.setElevation(8f); // Better elevation for TV
            backButton.setFocusable(true);
            backButton.setFocusableInTouchMode(true);
            FrameLayout.LayoutParams buttonParams = new FrameLayout.LayoutParams(
                200, // Width in pixels (matches tvOS 200 points)
                50   // Height in pixels (matches tvOS 50 points)
            );
            buttonParams.leftMargin = 50; // Left margin in pixels (matches tvOS 50 points)
            buttonParams.topMargin = 50;  // Top margin in pixels (matches tvOS 50 points)
            backButton.setLayoutParams(buttonParams);
            backButton.setOnClickListener(v -> closeView());

            // Add views to layout (ImageView behind, Button in front)
            mainLayout.addView(imageView);
            mainLayout.addView(backButton);
            backButton.bringToFront(); // Ensure button is drawn on top
            android.util.Log.d("WebViewPlugin", "Back button visibility: " + backButton.getVisibility()); // Log for debugging

            activity.addContentView(mainLayout, new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
            ));

            if (imageUrl != null && !imageUrl.isEmpty()) {
                new LoadImageTask(successCallback, failureCallback).execute(imageUrl);
                focusView();
                pauseUnity();
            } else {
                UnityPlayer.UnitySendMessage(gameObjectName, failureCallback, "No image URL provided");
                closeView();
            }
        });
    }

    // Sets up the main layout with back button handling for WebView
    private static void setupMainLayout(Activity activity) {
        LinearLayout linearLayout = new LinearLayout(activity) {
            @Override
            public boolean dispatchKeyEvent(KeyEvent event) {
                if (event.getAction() == KeyEvent.ACTION_DOWN && event.getKeyCode() == KeyEvent.KEYCODE_BACK) {
                    closeView();
                    return true;
                }
                return super.dispatchKeyEvent(event);
            }
        };
        linearLayout.setOrientation(LinearLayout.VERTICAL);
        linearLayout.setLayoutParams(new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MATCH_PARENT,
            ViewGroup.LayoutParams.MATCH_PARENT
        ));
        linearLayout.setBackgroundColor(0xFF000000);
        mainLayout = linearLayout; // Assign to ViewGroup field
    }

    // Creates the navigation bar with a back button for WebView
    private static LinearLayout createNavBar(Activity activity) {
        LinearLayout navBar = new LinearLayout(activity);
        navBar.setOrientation(LinearLayout.HORIZONTAL);
        navBar.setLayoutParams(new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MATCH_PARENT,
            dpToPx(60, activity)
        ));
        navBar.setBackgroundColor(0xFFCCCCCC);

        Button backButton = new Button(activity);
        backButton.setText("< Back");
        backButton.setTextColor(0xFF000000);
        backButton.setBackgroundColor(0xFFFFFFFF);
        backButton.setTextSize(20);
        LinearLayout.LayoutParams buttonParams = new LinearLayout.LayoutParams(
            dpToPx(200, activity),
            dpToPx(50, activity)
        );
        buttonParams.leftMargin = dpToPx(20, activity);
        buttonParams.topMargin = dpToPx(5, activity);
        backButton.setLayoutParams(buttonParams);
        backButton.setOnClickListener(v -> closeView());

        navBar.addView(backButton);
        return navBar;
    }

    // Sets focus to the back button
    private static void focusView() {
        if (mainLayout != null && mainLayout.getChildCount() > 0) {
            // For ImageView, back button is a direct child; for WebView, it's in navBar
            View backButton = mainLayout.getChildAt(mainLayout.getChildCount() - 1);
            if (backButton instanceof Button) {
                backButton.requestFocus();
                backButton.setFocusableInTouchMode(true);
            }
        }
    }

    private static void pauseUnity() {
        UnityPlayer.UnitySendMessage(gameObjectName, "PauseUnity", "");
    }

    private static void resumeUnity() {
        UnityPlayer.UnitySendMessage(gameObjectName, "ResumeUnity", "");
    }

    // Closes the current view (WebView or ImageView)
    private static void closeView() {
        UnityPlayer.UnitySendMessage(gameObjectName, WebViewPlugin.closeCallback, "The view was closed");

        if (mainLayout != null) {
            ((ViewGroup) mainLayout.getParent()).removeView(mainLayout);
            if (webView != null) {
                webView.destroy();
                webView = null;
            }
            imageView = null;
            mainLayout = null;
            resumeUnity();
        }
    }

    private static int dpToPx(int dp, Activity activity) {
        return Math.round(dp * activity.getResources().getDisplayMetrics().density);
    }

    // AsyncTask to load image from URL
    private static class LoadImageTask extends AsyncTask<String, Void, Bitmap> {
        private final String successCallback;
        private final String failureCallback;

        LoadImageTask(String successCallback, String failureCallback) {
            this.successCallback = successCallback;
            this.failureCallback = failureCallback;
        }

        @Override
        protected Bitmap doInBackground(String... urls) {
            String imageUrl = urls[0];
            try {
                URL url = new URL(imageUrl);
                HttpURLConnection connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);
                connection.connect();
                InputStream input = connection.getInputStream();
                Bitmap bitmap = BitmapFactory.decodeStream(input);
                input.close();
                connection.disconnect();
                return bitmap;
            } catch (Exception e) {
                android.util.Log.e("WebViewPlugin", "Failed to load image: " + e.getMessage());
                return null;
            }
        }

        @Override
        protected void onPostExecute(Bitmap bitmap) {
            if (bitmap != null && imageView != null) {
                imageView.setImageBitmap(bitmap);
                UnityPlayer.UnitySendMessage(gameObjectName, successCallback, "Image loaded successfully");
            } else {
                UnityPlayer.UnitySendMessage(gameObjectName, failureCallback, "Failed to load image");
                closeView();
            }
        }
    }
}