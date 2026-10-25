###
# --dataset: You chose yelp_labelled.txt as the dataset (internally, the CLI will split the one dataset into training and testing datasets).
# --label-col: You must specify the target column you want to predict (or the Label). In this case, you want to predict the sentiment in the second column (zero-indexed columns means this is column "1").
# --has-header: Use this option to specify if the dataset has a header. In this case, the dataset doesn't have a header, so it's false.
# --name: Use this option to provide a name for your machine learning model and related assets. In this case, all assets associated with this machine learning model will have SentimentModel in the name.
# --train-time: You must also specify the amount of time you'd like the ML.NET CLI to explore different models. In this case, 60 seconds (you can try increasing this number if no models are found after training). Note that for larger datasets, you should set a longer training time.
###
  
mlnet classification --dataset "yelp_labelled.txt" --label-col 1 --has-header false --name SentimentModel  --train-time 60
