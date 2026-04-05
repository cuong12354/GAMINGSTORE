document.addEventListener("DOMContentLoaded", () => {
	const chatbot = document.getElementById("salesChatbot");
	const panel = document.getElementById("salesChatMessages");
	const form = document.getElementById("salesChatForm");
	const input = document.getElementById("salesChatInput");
	const toggle = document.getElementById("salesChatToggle");

	if (!chatbot || !panel || !form || !input || !toggle) {
		return;
	}

	const chatUrl = chatbot.dataset.salesChatUrl;
	let hasBooted = false;

	const formatCurrency = (value) => new Intl.NumberFormat("vi-VN").format(value) + " đ";

	const scrollToBottom = () => {
		panel.scrollTop = panel.scrollHeight;
	};

	const addMessage = (content, sender) => {
		const bubble = document.createElement("div");
		bubble.className = `sales-chat-message ${sender}`;
		bubble.textContent = content;
		panel.appendChild(bubble);
		scrollToBottom();
	};

	const addTyping = () => {
		const bubble = document.createElement("div");
		bubble.className = "sales-chat-message bot";
		bubble.dataset.typing = "true";
		bubble.innerHTML = '<span class="sales-chat-typing"><span></span><span></span><span></span></span>';
		panel.appendChild(bubble);
		scrollToBottom();
		return bubble;
	};

	const addSuggestions = (suggestions) => {
		if (!Array.isArray(suggestions) || suggestions.length === 0) {
			return;
		}

		const wrapper = document.createElement("div");
		wrapper.className = "sales-chat-suggestions";

		suggestions.forEach((suggestion) => {
			const button = document.createElement("button");
			button.type = "button";
			button.className = "sales-chat-suggestion";
			button.textContent = suggestion;
			button.addEventListener("click", () => {
				input.value = suggestion;
				form.requestSubmit();
			});
			wrapper.appendChild(button);
		});

		panel.appendChild(wrapper);
		scrollToBottom();
	};

	const addProducts = (products) => {
		if (!Array.isArray(products) || products.length === 0) {
			return;
		}

		const list = document.createElement("div");
		list.className = "sales-chat-product-list";

		products.forEach((product) => {
			const link = document.createElement("a");
			link.className = "sales-chat-product";
			link.href = product.url;

			const categoryText = Array.isArray(product.categories) && product.categories.length > 0
				? product.categories.join(" • ")
				: "Sản phẩm gaming";

			link.innerHTML = `
				<img src="${product.imageUrl}" alt="${product.name}">
				<div>
					<div class="sales-chat-product-name">${product.name}</div>
					<div class="sales-chat-product-meta">${categoryText}</div>
					<div class="sales-chat-product-price">${formatCurrency(product.price)}</div>
				</div>
			`;

			list.appendChild(link);
		});

		panel.appendChild(list);
		scrollToBottom();
	};

	const bootChat = () => {
		if (hasBooted) {
			return;
		}

		hasBooted = true;
		addMessage("Chào bạn, mình là trợ lý bán hàng của GAMINGSTORE. Bạn cứ hỏi theo tên sản phẩm, tầm giá hoặc danh mục, mình sẽ gợi ý ngay.", "bot");
		addSuggestions(["Laptop gaming dưới 20 triệu", "Chuột gaming", "Màn hình", "Sản phẩm bán chạy"]);
	};

	toggle.addEventListener("click", () => {
		chatbot.classList.toggle("is-open");
		if (chatbot.classList.contains("is-open")) {
			bootChat();
			input.focus();
		}
	});

	form.addEventListener("submit", async (event) => {
		event.preventDefault();

		const message = input.value.trim();
		if (!message || !chatUrl) {
			return;
		}

		bootChat();
		addMessage(message, "user");
		input.value = "";

		const typingBubble = addTyping();

		try {
			const response = await fetch(chatUrl, {
				method: "POST",
				headers: {
					"Content-Type": "application/json"
				},
				body: JSON.stringify({ message })
			});

			const payload = await response.json();
			typingBubble.remove();

			if (!response.ok) {
				throw new Error("Chat request failed");
			}

			addMessage(payload.message || "Mình chưa thể trả lời lúc này, bạn thử lại giúp mình nhé.", "bot");
			addProducts(payload.products || []);
			addSuggestions(payload.suggestions || []);
		} catch (error) {
			typingBubble.remove();
			addMessage("Hiện chatbot đang bận một chút. Bạn thử lại sau hoặc tìm sản phẩm bằng ô tìm kiếm trên đầu trang nhé.", "bot");
		}
	});
});
