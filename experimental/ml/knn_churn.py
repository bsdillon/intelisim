import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from sklearn.neighbors import KNeighborsClassifier
from sklearn.model_selection import train_test_split

rng = np.random.default_rng(0)
n = 400
account_length = rng.normal(100, 40, n).clip(1, 250)
service_calls = rng.poisson(2, n) + (account_length < 60).astype(int)
# More calls and shorter accounts churn more often. Noise is intentional.
score = 0.04 * service_calls - 0.01 * (account_length - 100)
churn = (score + rng.normal(0, 0.15, n) > 0.05).astype(int)

churn_df = pd.DataFrame({
    "account_length": account_length,
    "customer_service_calls": service_calls,
    "churn": churn,
})

y = churn_df["churn"].values
X = churn_df[["account_length", "customer_service_calls"]].values

knn = KNeighborsClassifier(n_neighbors=6)
knn.fit(X, y)

X_new = np.array([[30.0, 1.0], [107.0, 24.1], [213.0, 10.9]])
print("Predictions:", knn.predict(X_new))

X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.3, random_state=0, stratify=y
)

neighbors = np.arange(1, 21)
train_accuracies = []
test_accuracies = []
for k in neighbors:
    model = KNeighborsClassifier(n_neighbors=k)
    model.fit(X_train, y_train)
    train_accuracies.append(model.score(X_train, y_train))
    test_accuracies.append(model.score(X_test, y_test))

plt.title("KNN: Varying Number of Neighbors")
plt.plot(neighbors, train_accuracies, label="Training Accuracy")
plt.plot(neighbors, test_accuracies, label="Testing Accuracy")
plt.legend()
plt.xlabel("Number of Neighbors")
plt.ylabel("Accuracy")
plt.xticks(neighbors)
plt.tight_layout()
# plt.show()
plt.savefig("complexity.png")
print("wrote complexity.png")